using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Archive;
using Cethleann.Structure.KTID;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    /// <summary>
    ///     Parser for RDB NAME databases
    /// </summary>
    [PublicAPI]
    public class NAME
    {
        /// <summary>
        ///     Initialize with no data (for compatability)
        /// </summary>
        public NAME() { }

        /// <summary>
        ///     Initialize with stream data
        /// </summary>
        /// <param name="buffer"></param>
        public NAME(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<NAMEHeader>(buffer);
            Entries = new List<(NAMEEntry entry, string[] strings)>(Header.Count);
            NameMap = new Dictionary<uint, string>(Header.Count);
            ExtMap = new Dictionary<uint, string>(Header.Count);
            ExtMapRaw = new Dictionary<uint, string>(Header.Count);

            var offset = Header.SectionHeader.Size;
            var entrySize = SizeHelper.SizeOf<NAMEEntry>();
            for (var i = 0; i < Header.Count; ++i)
            {
                var entry = MemoryMarshal.Read<NAMEEntry>(buffer.Slice(offset));
                var strings = new string[entry.Count];
                if (entry.Count > 0)
                {
                    var pointers = MemoryMarshal.Cast<byte, int>(buffer.Slice(offset + entrySize, entry.Count * 4));
                    for (var index = 0; index < pointers.Length; index++)
                    {
                        var pointer = pointers[index];
                        strings[index] = buffer.Slice(offset + pointer).ReadString();
                    }
                }

                offset += entry.SectionHeader.Size;
                offset = offset.Align(4);

                Entries.Add((entry, strings));

                var (name, ext) = RDB.StripName(strings[0]);
                NameMap[entry.KTID] = name;

                var hash = RDB.Hash(strings[1]);
                if (ext != null) ExtMap[hash] = ext.ToLower();

                ExtMapRaw[hash] = strings[1].Split(':').Last();
            }
        }

        /// <summary>
        ///     NAME Header
        /// </summary>
        public NAMEHeader Header { get; set; }

        /// <summary>
        ///     Lsof entries with strings
        /// </summary>
        public List<(NAMEEntry entry, string[] strings)> Entries { get; set; } = new List<(NAMEEntry entry, string[] strings)>();

        /// <summary>
        ///     Hashes mapped to strings
        /// </summary>
        public Dictionary<uint, string> NameMap { get; set; } = new Dictionary<uint, string>();

        /// <summary>
        ///     Type infos hashes mapped to guessed extensions
        /// </summary>
        public Dictionary<uint, string> ExtMap { get; set; } = new Dictionary<uint, string>();

        /// <summary>
        ///     Type info hash map
        /// </summary>
        public Dictionary<uint, string> ExtMapRaw { get; set; } = new Dictionary<uint, string>();
    }
}
