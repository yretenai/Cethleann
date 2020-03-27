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
    public class NDB : RDBINFO
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
            Entries = new List<(KTIDReference reference, string[] strings)>(Header.Count);
            NameMap = new Dictionary<KTIDReference, string>(Header.Count);
            HashMap = new Dictionary<KTIDReference, string>(Header.Count);
            ExtMap = new Dictionary<KTIDReference, string>(Header.Count);

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

                Entries.Add((entry.KTID, strings));

                var (name, ext) = RDB.StripName(strings[0]);
                NameMap[entry.KTID] = name;
                HashMap[entry.KTID] = strings[0];

                var hash = RDB.Hash(strings[1]);
                if (ext != null) ExtMap[hash] = ext.ToLower();

                foreach (var str in strings) HashMap[RDB.Hash(str)] = str;
            }
        }

        /// <summary>
        ///     NDB Header
        /// </summary>
        public NDBHeader Header { get; set; }
    }
}
