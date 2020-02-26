using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Archive;
using Cethleann.Structure.KTID;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    /// <summary>
    ///     Parser for RDB NAME databases (NDB)
    /// </summary>
    [PublicAPI]
    public class NDB
    {
        /// <summary>
        ///     Initialize with no data (for compatability)
        /// </summary>
        public NDB() { }

        /// <summary>
        ///     Initialize with stream data
        /// </summary>
        /// <param name="buffer"></param>
        public NDB(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<NDBHeader>(buffer);
            Entries = new List<(NDBEntry entry, string[] strings)>(Header.Count);
            NameMap = new Dictionary<uint, string>(Header.Count);
            HashMap = new Dictionary<uint, string>(Header.Count);
            ExtMap = new Dictionary<uint, string>(Header.Count);

            var offset = Header.SectionHeader.Size;
            var entrySize = SizeHelper.SizeOf<NDBEntry>();
            for (var i = 0; i < Header.Count; ++i)
            {
                var entry = MemoryMarshal.Read<NDBEntry>(buffer.Slice(offset));
                var strings = new string[entry.Count];
                Logger.Assert(entry.Count >= 2, "entry.Count >= 2");
                if (entry.Count > 0)
                {
                    var pointers = MemoryMarshal.Cast<byte, int>(buffer.Slice(offset + entrySize, entry.Count * 4));
                    for (var index = 0; index < pointers.Length; index++)
                    {
                        var pointer = pointers[index];
                        strings[index] = buffer.Slice(offset + pointer).ReadString() ?? string.Empty;
                    }
                }

                offset += entry.SectionHeader.Size;
                offset = offset.Align(4);

                Entries.Add((entry, strings));

                var (name, ext) = RDB.StripName(strings[0]);
                NameMap[entry.KTID] = name;
                HashMap[entry.KTID] = strings[0];

                var hash = RDB.Hash(strings[1]);
                if (ext != null) ExtMap[hash] = ext.ToLower();

                HashMap[hash] = strings[1];
            }
        }

        /// <summary>
        ///     NDB Header
        /// </summary>
        public NDBHeader Header { get; set; }

        /// <summary>
        ///     Lsof entries with strings
        /// </summary>
        public List<(NDBEntry entry, string[] strings)> Entries { get; set; } = new List<(NDBEntry entry, string[] strings)>();

        /// <summary>
        ///     Hashes mapped to strings
        /// </summary>
        public Dictionary<uint, string> NameMap { get; set; } = new Dictionary<uint, string>();

        /// <summary>
        ///     Hashes of both names and typeinfos.
        /// </summary>
        public Dictionary<uint, string> HashMap { get; set; } = new Dictionary<uint, string>();

        /// <summary>
        ///     Type infos hashes mapped to guessed extensions
        /// </summary>
        public Dictionary<uint, string> ExtMap { get; set; } = new Dictionary<uint, string>();
    }
}
