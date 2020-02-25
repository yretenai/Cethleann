using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Pack;
using JetBrains.Annotations;

namespace Cethleann.Pack
{
    /// <summary>
    ///     KTGL Animation Pack
    /// </summary>
    [PublicAPI]
    public class GAPK
    {
        /// <summary>
        ///     Initialize with data
        /// </summary>
        /// <param name="data"></param>
        public GAPK(Span<byte> data)
        {
            Header = MemoryMarshal.Read<GAPKHeader>(data);
            var offsets = MemoryMarshal.Cast<byte, int>(data.Slice(Header.PointerTablePointer, Header.PointerTableCount * 4));
            var sizes = MemoryMarshal.Cast<byte, int>(data.Slice(Header.SizeTablePointer, Header.SizeTableCount * 4));
            NameMap = new GPKNameMap(data.Slice(Header.IndexTablePointer));
            for (var i = 0; i < NameMap.Entries.Length; ++i) Blobs.Add(new Memory<byte>(data.Slice(offsets[i], sizes[i]).ToArray()));
        }

        /// <summary>
        ///     GAPK Header
        /// </summary>
        public GAPKHeader Header { get; set; }

        /// <summary>
        ///     GAPK Data Blobs
        /// </summary>
        public List<Memory<byte>> Blobs { get; set; } = new List<Memory<byte>>();

        /// <summary>
        ///     Name Map for the files stored within
        /// </summary>
        public GPKNameMap NameMap { get; set; }
    }
}
