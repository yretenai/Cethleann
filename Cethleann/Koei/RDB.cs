using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Cethleann.Structure;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Koei
{
    /// <summary>
    ///     First seen in DoA6, but it's used across the company.
    ///     It looks like it's the go-to format for KTGL Version 2 / "NewSoftEngine"
    /// </summary>
    [PublicAPI]
    public class RDB : IDisposable
    {
        private static readonly Regex AddressRegex = new Regex("([a-fA-F0-9]*)@([a-fA-F0-9]*)(?:#([a-fA-F0-9]*))?(?:\\$([a-fA-F0-9])*)?");

        public RDB(Span<byte> buffer, string name, string directory)
        {
            Name = name;
            Header = MemoryMarshal.Read<RDBHeader>(buffer);
            RDBDirectory = directory;
            Directory = Path.Combine(directory, buffer.Slice(SizeHelper.SizeOf<RDBHeader>(), Header.HeaderSize - SizeHelper.SizeOf<RDBHeader>()).ReadString());

            var offset = Header.HeaderSize;
            for (var i = 0; i < Header.Count; ++i)
            {
                var (entry, typeblob, data) = ReadRDBEntry(buffer.Slice(offset));
                offset += (int) entry.EntrySize.Align(4);
                Entries.Add((entry, typeblob, DecodeOffset(((Span<byte>) data).ReadString())));
            }
        }

        public Dictionary<string, Stream> Streams { get; set; }
        public string RDBDirectory { get; set; }
        public string Directory { get; set; }
        public string Name { get; set; }
        public RDBHeader Header { get; set; }
        public List<(RDBEntry entry, byte[] typeBuffer, (long offset, long size, int binId, int binSubId))> Entries { get; set; } = new List<(RDBEntry, byte[], (long, long, int, int))>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public RDBEntry GetEntry(int index)
        {
            if (index >= Entries.Count) throw new IndexOutOfRangeException();

            return Entries[index].entry;
        }

        public string GetBinPath(int index)
        {
            if (index >= Entries.Count) throw new IndexOutOfRangeException();

            var (_, _, (_, _, binId, binSub)) = Entries[index];
            var binPath = Path.Combine(RDBDirectory, Name + ".rdb.bin");
            if (binId > -1) binPath += binId.ToString();
            if (binSub > -1) binPath += $"_{binSub}";
            return binPath;
        }

        public string GetExternalPath(int fileId)
        {
            return Path.Combine(Directory, $"0x{fileId:x8}.file");
        }

        public Memory<byte> ReadEntry(int index)
        {
            GC.ReRegisterForFinalize(this);
            var (entry, _, (offset, size, _, _)) = Entries[index];
            Span<byte> blob;
            if (entry.Flags.HasFlag(RDBFlags.External))
            {
                var path = GetExternalPath(entry.FileId);
                if (!File.Exists(path))
                {
                    Logger.Error("RDB", $"Cannot find external RDB file {Path.GetFileName(path)}");
                    return Memory<byte>.Empty;
                }

                if (offset > -1)
                {
                    using var stream = File.OpenRead(path);
                    stream.Position = offset;
                    blob = new Span<byte>(new byte[size]);
                    stream.Read(blob);
                }
                else
                {
                    blob = File.ReadAllBytes(path);
                }
            }
            else
            {
                var path = GetBinPath(index);
                if (!Streams.TryGetValue(Path.GetFileName(path), out var stream))
                {
                    if (!File.Exists(path))
                    {
                        Logger.Error("RDB", $"Cannot find RDBIN {Path.GetFileName(path)}");
                        return Memory<byte>.Empty;
                    }

                    stream = File.OpenRead(path);
                    Streams[Path.GetFileName(path)] = stream;
                }

                stream.Position = offset;
                blob = new Span<byte>(new byte[size]);
            }

            if (blob.Length < SizeHelper.SizeOf<RDBEntry>()) return Memory<byte>.Empty;

            var (fileEntry, _, buffer) = ReadRDBEntry(blob);
            return new Memory<byte>(entry.Flags.HasFlag(RDBFlags.Compressed) ? Omega.Compression.Decompress(buffer, (int) fileEntry.Size).ToArray() : buffer);
        }

        private (RDBEntry entry, byte[] typeblob, byte[] data) ReadRDBEntry(Span<byte> buffer)
        {
            var entry = MemoryMarshal.Read<RDBEntry>(buffer);
            var unknowns = buffer.Slice(SizeHelper.SizeOf<RDBEntry>(), (int) (entry.EntrySize - SizeHelper.SizeOf<RDBEntry>() - entry.ContentSize)).ToArray();
            var data = buffer.Slice((int) (entry.EntrySize - entry.ContentSize), (int) entry.ContentSize).ToArray();
            return (entry, unknowns, data);
        }

        private static (long offset, long size, int binId, int binSubId) DecodeOffset(string packed)
        {
            if (packed == null) return (-1, -1, -1, -1);
            var regex = AddressRegex.Match(packed);
            if (!regex.Success) return (-1, -1, -1, -1);

            var groups = regex.Groups;
            var offset = long.Parse(groups[1].Value, NumberStyles.HexNumber);
            var size = long.Parse(groups[2].Value, NumberStyles.HexNumber);
            var binId = groups[3].Value != "" ? int.Parse(groups[2].Value, NumberStyles.HexNumber) : -1;
            var binSubId = groups[4].Value != "" ? int.Parse(groups[2].Value, NumberStyles.HexNumber) : -1;

            return (offset, size, binId, binSubId);
        }

        ~RDB()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            foreach (var stream in Streams.Values) stream.Dispose();

            Streams.Clear();
        }
    }
}
