using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Archive;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Archive
{
    /// <summary>
    ///     lfm_order_*.bin archive files
    /// </summary>
    [PublicAPI]
    public class LINKNAME
    {
        /// <summary>
        ///     Read from span
        /// </summary>
        /// <param name="buffer"></param>
        public LINKNAME(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<LFMOrderHeader>(buffer);
            Entries = MemoryMarshal.Cast<byte, LFMOrderEntry>(buffer.Slice(Header.DataPointer, SizeHelper.SizeOf<LFMOrderEntry>() * Header.Count)).ToArray();
            SourceName = buffer.Slice(Header.SourceNamePointer).ReadString();
            foreach (var entry in Entries) Names.Add(buffer.Slice(entry.Pointer).ReadString()?.Substring(1));
        }

        /// <summary>
        ///     Source file name
        /// </summary>
        public string? SourceName { get; set; }

        /// <summary>
        ///     Lsof names for the files
        /// </summary>
        public List<string?> Names { get; set; } = new List<string?>();

        /// <summary>
        ///     LFM Order entry list
        /// </summary>
        public LFMOrderEntry[] Entries { get; set; }

        /// <summary>
        ///     LFM Order Header
        /// </summary>
        public LFMOrderHeader Header { get; set; }

        /// <summary>
        ///     Get a filename for an id.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string? GetName(int index)
        {
            for (var i = 0; i < Entries.Length; i++)
            {
                var entry = Entries[i];
                if (entry.FileId == index) return Names[i];
            }

            return null;
        }
    }
}
