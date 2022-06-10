using Cethleann.Compression;
using Cethleann.KTID;
using Cethleann.Structure;
using Cethleann.Structure.KTID;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ScrambleRDBEncryption = Cethleann.Compression.Scramble.RDBEncryption;
using ScrambleSRSTEncryption = Cethleann.Compression.Scramble.SRSTEncryption;

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
        private static readonly Regex ADDRESS_REGEX = new Regex(@"([a-fA-F0-9]*)@([a-fA-F0-9]*)(?:#([a-fA-F0-9]*))?(?:\&([a-fA-F0-9]*))?(?:\?(.*))?");
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
        /// <param name="game"></param>
        public RDB(Span<byte> buffer, string name, string directory, string game)
        {
            Name = name;
            Game = game;

            if(File.Exists(Path.Combine(directory, name + ".rdx")))
                External = new RDX(File.ReadAllBytes(Path.Combine(directory, name + ".rdx")), directory);

            Header = MemoryMarshal.Read<RDBHeader>(buffer);
            RDBDirectory = directory;
            DataDirectory = buffer.Slice(SizeHelper.SizeOf<RDBHeader>(), Header.HeaderSize - SizeHelper.SizeOf<RDBHeader>()).ReadString() ?? string.Empty;
            Directory = Path.Combine(directory, DataDirectory);

            var offset = Header.HeaderSize;
            for (var i = 0; i < Header.Count; ++i)
            {
                var (entry, typeblob, data) = ReadRDBEntry(buffer.Slice(offset), true);
                var fileEntry = entry.GetValueOrDefault();
                offset += (int) fileEntry.EntrySize.Align(4);
                Entries.Add(External?.KTIDToEntryId.ContainsKey(fileEntry.FileKTID) == true ? (fileEntry, typeblob, (-1, -1, -1, -1, null)) : (fileEntry, typeblob, DecodeOffset(((Span<byte>) data).ReadString() ?? "0")));
                KTIDToEntryId[fileEntry.FileKTID] = i;
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
        ///     Absolute path to external data directory
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        ///     External data directory
        /// </summary>
        public string DataDirectory { get; set; }

        /// <summary>
        ///     Name of this archive
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Name of this game
        /// </summary>
        public string Game { get; set; }

        /// <summary>
        ///     RDB Header
        /// </summary>
        public RDBHeader Header { get; set; }

        /// <summary>
        ///     List of RDB entries found in this file
        /// </summary>
        public List<(RDBEntry entry, byte[]? typeBuffer, (long offset, long size, int binId, int binSubId, string? filePath))> Entries { get; set; } = new List<(RDBEntry, byte[]?, (long, long, int, int, string?))>();

        /// <summary>
        ///     KTID to Entry ID map
        /// </summary>
        public Dictionary<KTIDReference, int> KTIDToEntryId { get; set; } = new Dictionary<KTIDReference, int>();

        /// <summary>
        ///     Name Database for this RDB
        /// </summary>
        public RDBINFO NameDatabase { get; set; } = new NDB();
        
        /// <summary>
        ///     External PRDBs 
        /// </summary>
        public RDX? External { get; set; }

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

            var (_, _, (_, _, binId, binSub, _)) = Entries[index];
            var binPath = Path.Combine(RDBDirectory, Name + ".rdb.bin");
            if (binId > -1) binPath += binId.ToString();
            if (binSub > -1) binPath += $"_{binSub}";
            return binPath;
        }

        private static bool CheckFileExistsHashed(ref string path)
        {
            if (File.Exists(path)) return true;
            var dir = Path.GetDirectoryName(path) ?? "";
            var files = System.IO.Directory.GetFiles(dir, Path.GetFileName(path) + ".*", SearchOption.TopDirectoryOnly);
            if (files.Length <= 0) return false;
            path = files[0];
            return true;
        }

        /// <summary>
        ///     Gets an external file path for a given index
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string GetExternalPath(KTIDReference fileId, string? filePath = null)
        {
            while (true)
            {
                if (filePath == null)
                {
                    var fileId1 = fileId;
                    filePath = $"0x{fileId1:x8}.file";
                    continue;
                }

                var test = Path.Combine(Directory, filePath);
                if (System.IO.Directory.Exists(Directory) && CheckFileExistsHashed(ref test)) return test;
                test = Path.Combine(RDBDirectory, filePath);
                if (CheckFileExistsHashed(ref test)) return test;
                test = Path.Combine(RDBDirectory, $"{DataDirectory.Replace('/', '_')}{filePath}");
                return CheckFileExistsHashed(ref test) ? test : Path.Combine(Directory, filePath);
            }
        }

        /// <summary>
        ///     Reads a file by the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Memory<byte> ReadEntry(int index)
        {
            if (index < 0) return Memory<byte>.Empty;

            var (entry, _, (offset, size, binId, binSub, filePath)) = Entries[index];

            Span<byte> blob;
            if (External != null && External.KTIDToEntryId.TryGetValue(entry.FileKTID, out var eId))
            {
                blob = External.ReadEntry(eId).Span;
            }
            else if (entry.Flags.HasFlag(RDBFlags.External) || !string.IsNullOrEmpty(filePath))
            {
                if (filePath != null) // not checked or used, but just in case they decide to :-)
                {
                    if (binId > -1) filePath += binId.ToString();
                    if (binSub > -1) filePath += $"_{binSub}";
                }

                var path = GetExternalPath(entry.FileKTID, filePath);
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

            var (fileEntry, _, buffer) = ReadRDBEntry(blob, true);
            var fileEntryA = fileEntry.GetValueOrDefault();
            if (fileEntryA.Size == 0) return Memory<byte>.Empty;

            if (Game == "Scramble")
            {
                // todo: check KTID Type.
                if (MemoryMarshal.Read<uint>(buffer) == 0x53525354)
                {
                    ScrambleSRSTEncryption.Decrypt(buffer);
                }
                else if (entry.Flags.HasFlag(RDBFlags.ZlibCompressed) && entry.Flags.HasFlag(RDBFlags.Encrypted))
                {
                    ScrambleRDBEncryption.Decrypt(buffer, entry.FileKTID.KTID);
                    entry.Flags ^= RDBFlags.Encrypted;
                }
            }

            if (entry.Flags.HasFlag(RDBFlags.ZlibCompressed) || entry.Flags.HasFlag(RDBFlags.Lz4Compressed))
                return StreamCompression.Decompress(buffer, new CompressionOptions
                {
                    Length = (int) fileEntryA.Size,
                    Type = (DataCompression) ((int) entry.Flags >> 20 & 0xF)
                }).ToArray();
            return buffer;
        }

        internal static (RDBEntry? entry, byte[]? typeblob, byte[]? data) ReadRDBEntry(Span<byte> buffer, bool readData)
        {
            var entry = MemoryMarshal.Read<RDBEntry>(buffer);
            if (entry.Magic != DataType.RDBIndex) return (null, null, null);
            var unknownsSize = entry.EntrySize - SizeHelper.SizeOf<RDBEntry>() - entry.ContentSize;
            var unknowns = unknownsSize < 1 ? Array.Empty<byte>() : buffer.Slice(SizeHelper.SizeOf<RDBEntry>(), (int) unknownsSize).ToArray();
            var data = readData ? buffer.Slice((int) (entry.EntrySize - entry.ContentSize), (int) entry.ContentSize).ToArray() : Array.Empty<byte>();
            return (entry, unknowns, data);
        }

        private static (long offset, long size, int binId, int binSubId, string? filePath) DecodeOffset(string packed)
        {
            try
            {
                var regex = ADDRESS_REGEX.Match(packed);
                if (!regex.Success) return (-1, -1, -1, -1, null);

                var groups = regex.Groups;
                var offset = long.Parse(groups[1].Value, NumberStyles.HexNumber);
                var size = long.Parse(groups[2].Value, NumberStyles.HexNumber);
                var binId = groups[3].Value != string.Empty ? int.Parse(groups[3].Value, NumberStyles.HexNumber) : -1;
                var binSubId = groups[4].Value != string.Empty ? int.Parse(groups[4].Value, NumberStyles.HexNumber) : -1;
                var filePath = groups[5].Value != string.Empty ? groups[5].Value : null;
                return (offset, size, binId, binSubId, filePath);
            }
            catch
            {
                return (-1, -1, -1, -1, null);
            }
        }

        /// <summary>
        ///     Disposes
        /// </summary>
        ~RDB() => Dispose(false);

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
        public static KTIDReference Hash(string text, string ext, string prefix = "R_") => Hash(Encoding.UTF8.GetBytes(text), ext, prefix);

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
        public static KTIDReference Hash(string text) => Hash(Encoding.UTF8.GetBytes(text.Substring(1)), text[0] * HASH_KEY, HASH_KEY);

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
