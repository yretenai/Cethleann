using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Cethleann.Structure.Pack;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Pack
{
    /// <summary>
    ///     GAPK/GMPK NameMap/MotionMap parser
    /// </summary>
    [PublicAPI]
    public class GPKNameMap
    {
        /// <summary>
        ///     Initialize with binary data
        /// </summary>
        /// <param name="data"></param>
        public GPKNameMap(Span<byte> data)
        {
            var offset = 8;
            Name = data.Slice(0, 8).ReadString() ?? string.Empty;
            Header = MemoryMarshal.Read<GPKNameMapHeader>(data.Slice(offset));
            offset += SizeHelper.SizeOf<GPKNameMapHeader>();
            Entries = MemoryMarshal.Cast<byte, GPKNameMapEntry>(data.Slice(offset, SizeHelper.SizeOf<GPKNameMapEntry>() * Header.Count)).ToArray();
            offset += SizeHelper.SizeOf<GPKNameMapEntry>() * Header.Count;
            var cast = MemoryMarshal.Cast<byte, short>(data.Slice(offset, Header.Count * 2 * 2));
            for (var index = 0; index < cast.Length; index += 2)
            {
                var pointer = cast[index];
                var length = data[offset + pointer];
                var suffix = Encoding.ASCII.GetString(data.Slice(offset + pointer + 1, length));
                pointer = cast[index + 1];
                length = data[offset + pointer];
                var prefix = Encoding.ASCII.GetString(data.Slice(offset + pointer + 1, length));
                Names.Add(prefix + suffix);
            }
        }

        /// <summary>
        ///     GPK Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     NameMap Header
        /// </summary>
        public GPKNameMapHeader Header { get; set; }

        /// <summary>
        ///     NameMap Entries
        /// </summary>
        public GPKNameMapEntry[] Entries { get; set; }

        /// <summary>
        ///     Assembled Names
        /// </summary>
        public List<string?> Names { get; set; } = new List<string?>();
    }
}
