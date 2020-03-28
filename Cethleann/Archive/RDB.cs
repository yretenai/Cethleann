using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Cethleann.Compression;
using Cethleann.KTID;
using Cethleann.Structure;
using Cethleann.Structure.KTID;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Archive
{
    /// <summary>
    ///     First seen in DoA6, but it's used across the company.
    ///     It looks like it's the go-to format for KTGL Version 2 / "NewSoftEngine"
    /// </summary>
    [PublicAPI]
    public class RDB : IDisposable
    {
        private const byte HASH_KEY = 0x1F;
        private static readonly Regex ADDRESS_REGEX = new Regex("([a-fA-F0-9]*)@([a-fA-F0-9]*)(?:#([a-fA-F0-9]*))?(?:\\&([a-fA-F0-9])*)?");
        private static byte[] HASH_PREFIX = { 0xEF, 0xBC, 0xBB };
        private static byte[] HASH_SUFFIX = { 0xEF, 0xBC, 0xBD };
        internal static readonly string HASH_PREFIX_STR = Encoding.UTF8.GetString(HASH_PREFIX);
        internal static readonly string HASH_SUFFIX_STR = Encoding.UTF8.GetString(HASH_SUFFIX);

        /// <summary>
        ///     Initialize with buffer and basic info about the FS
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="name"></param>
        /// <param name="directory"></param>
        public RDB(Span<byte> buffer, string name, string directory)
        {
            Name = name;
            Header = MemoryMarshal.Read<RDBHeader>(buffer);
            RDBDirectory = directory;
            Directory = Path.Combine(directory, buffer.Slice(SizeHelper.SizeOf<RDBHeader>(), Header.HeaderSize - SizeHelper.SizeOf<RDBHeader>()).ReadString() ?? string.Empty);

            var offset = Header.HeaderSize;
            for (var i = 0; i < Header.Count; ++i)
            {
                var (entry, typeblob, data) = ReadRDBEntry(buffer.Slice(offset));
                offset += (int) entry.EntrySize.Align(4);
                Entries.Add((entry, typeblob, DecodeOffset(((Span<byte>) data).ReadString() ?? "0")));
                KTIDToEntryId[entry.FileKTID] = i;
            }

            if (!KTIDToEntryId.TryGetValue(Header.NameDatabaseKTID, out var nameDatabaseId)) return;
            var nameBuffer = ReadEntry(nameDatabaseId);
            if (nameBuffer.Length == 0) return;
            NameDatabase = new NDB(nameBuffer.Span);
        }

        private Dictionary<string, Stream> Streams { get; set; } = new Dictionary<string, Stream>();

        /// <summary>
        ///     Directory that holds this RDB file
        /// </summary>
        public string RDBDirectory { get; set; }

        /// <summary>
        ///     External data directory
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        ///     Name of this RDB archive
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     RDB Header
        /// </summary>
        public RDBHeader Header { get; set; }

        /// <summary>
        ///     List of RDB entries found in this file
        /// </summary>
        public List<(RDBEntry entry, byte[]? typeBuffer, (long offset, long size, int binId, int binSubId))> Entries { get; set; } = new List<(RDBEntry, byte[]?, (long, long, int, int))>();

        /// <summary>
        ///     KTID to Entry ID map
        /// </summary>
        public Dictionary<KTIDReference, int> KTIDToEntryId { get; set; } = new Dictionary<KTIDReference, int>();

        /// <summary>
        ///     Name Database for this RDB
        /// </summary>
        public RDBINFO NameDatabase { get; set; } = new NDB();

        /// <summary>
        ///     Clean up streams
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Gets RDB Info Entry
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public RDBEntry GetEntry(int index)
        {
            if (index >= Entries.Count) throw new IndexOutOfRangeException();

            return Entries[index].entry;
        }

        /// <summary>
        ///     Gets a bin path for a given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public string GetBinPath(int index)
        {
            if (index >= Entries.Count) throw new IndexOutOfRangeException();

            var (_, _, (_, _, binId, binSub)) = Entries[index];
            var binPath = Path.Combine(RDBDirectory, Name + ".rdb.bin");
            if (binId > -1) binPath += binId.ToString();
            if (binSub > -1) binPath += $"_{binSub}";
            return binPath;
        }

        /// <summary>
        ///     Gets an external file path for a given index
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public string GetExternalPath(KTIDReference fileId)
        {
            return Path.Combine(Directory, $"0x{fileId:x8}.file");
        }

        /// <summary>
        ///     Reads a file by the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Memory<byte> ReadEntry(int index)
        {
            if (index < 0) return Memory<byte>.Empty;

            var (entry, _, (offset, size, _, _)) = Entries[index];

            Span<byte> blob;
            if (entry.Flags.HasFlag(RDBFlags.External))
            {
                var path = GetExternalPath(entry.FileKTID);
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

                    GC.ReRegisterForFinalize(this);
                    stream = File.OpenRead(path);
                    Streams[Path.GetFileName(path)] = stream;
                }

                stream.Position = offset;
                blob = new Span<byte>(new byte[size]);
                stream.Read(blob);
            }

            if (blob.Length < SizeHelper.SizeOf<RDBEntry>()) return Memory<byte>.Empty;

            var (fileEntry, _, buffer) = ReadRDBEntry(blob);
            if (fileEntry.Size == 0) return Memory<byte>.Empty;
            if (entry.Flags.HasFlag(RDBFlags.ZlibCompressed) || entry.Flags.HasFlag(RDBFlags.Lz4Compressed))
                return StreamCompression.Decompress(buffer, (int) fileEntry.Size, (DataCompression) ((int) entry.Flags >> 20 & 0xF)).ToArray();
            return buffer;
        }

        private (RDBEntry entry, byte[]? typeblob, byte[]? data) ReadRDBEntry(Span<byte> buffer)
        {
            var entry = MemoryMarshal.Read<RDBEntry>(buffer);
            if (entry.Magic != DataType.RDBIndex) return (default, null, null);
            var unknownsSize = entry.EntrySize - SizeHelper.SizeOf<RDBEntry>() - entry.ContentSize;
            var unknowns = unknownsSize < 1 ? new byte[0] : buffer.Slice(SizeHelper.SizeOf<RDBEntry>(), (int) (unknownsSize)).ToArray();
            var data = buffer.Slice((int) (entry.EntrySize - entry.ContentSize), (int) entry.ContentSize).ToArray();
            return (entry, unknowns, data);
        }

        private static (long offset, long size, int binId, int binSubId) DecodeOffset(string packed)
        {
            if (packed == null) return (-1, -1, -1, -1);
            var regex = ADDRESS_REGEX.Match(packed);
            if (!regex.Success) return (-1, -1, -1, -1);

            var groups = regex.Groups;
            var offset = long.Parse(groups[1].Value, NumberStyles.HexNumber);
            var size = long.Parse(groups[2].Value, NumberStyles.HexNumber);
            var binId = groups[3].Value != string.Empty ? int.Parse(groups[3].Value, NumberStyles.HexNumber) : -1;
            var binSubId = groups[4].Value != string.Empty ? int.Parse(groups[4].Value, NumberStyles.HexNumber) : -1;
            return (offset, size, binId, binSubId);
        }

        /// <summary>
        ///     Disposes
        /// </summary>
        ~RDB()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            foreach (var stream in Streams.Values) stream.Dispose();

            Streams.Clear();
        }

        /// <summary>
        ///     Hash filename
        /// </summary>
        /// <param name="text"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static KTIDReference Hash(Span<byte> text, int iv, int key)
        {
            unchecked
            {
                foreach (var ch in text)
                {
                    var state = key;
                    key *= HASH_KEY;
                    iv += HASH_KEY * state * (sbyte) ch;
                }

                return (uint) iv;
            }
        }

        /// <summary>
        ///     Hash filename given formatting
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ext"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static KTIDReference Hash(string text, string ext, string prefix = "R_")
        {
            return Hash(Encoding.UTF8.GetBytes(text), ext, prefix);
        }

        /// <summary>
        ///     Hash filename given formatting
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ext"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static KTIDReference Hash(Span<byte> text, string ext, string prefix = "R_")
        {
            Span<byte> buffer = new byte[ext.Length + prefix.Length + HASH_PREFIX.Length + HASH_SUFFIX.Length + text.Length];
            prefix.ToSpan().CopyTo(buffer);
            ext.ToSpan().CopyTo(buffer.Slice(prefix.Length));
            HASH_PREFIX.CopyTo(buffer.Slice(prefix.Length + ext.Length));
            text.CopyTo(buffer.Slice(prefix.Length + ext.Length + HASH_PREFIX.Length));
            HASH_SUFFIX.CopyTo(buffer.Slice(prefix.Length + ext.Length + HASH_PREFIX.Length + text.Length));
            return Hash(buffer.Slice(1), buffer[0] * HASH_KEY, HASH_KEY);
        }

        /// <summary>
        ///     Hash filenames
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static KTIDReference Hash(string text)
        {
            return Hash(Encoding.UTF8.GetBytes(text.Substring(1)), text[0] * HASH_KEY, HASH_KEY);
        }

        /// <summary>
        ///     Strips formatting from a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static (string name, string? ext) StripName(string text)
        {
            string? ext = null;
            var index = text.IndexOf(HASH_PREFIX_STR, StringComparison.Ordinal);
            if (text.StartsWith("R_") && index > 2) ext = text.Substring(2, index - 2);

            if (index > -1) text = text.Substring(index + 1);

            index = text.IndexOf(HASH_SUFFIX_STR, StringComparison.Ordinal);
            if (index > -1) text = text.Substring(0, index);

            return (text, ext);
        }
    }
}
