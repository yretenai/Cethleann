using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using JetBrains.Annotations;

namespace Cethleann.Pack
{
    /// <summary>
    ///     KTGL Animation Pack
    /// </summary>
    [PublicAPI]
    public class GMPK
    {
        /// <summary>
        ///     Initialize with data
        /// </summary>
        /// <param name="data"></param>
        public GMPK(Span<byte> data)
        {
            Header = MemoryMarshal.Read<GMPKHeader>(data);
            Index = MemoryMarshal.Read<GMPKIndex>(data.Slice(Header.NameMapIndexPointer));
            NameMap = new GPKNameMap(data.Slice(Index.NameMapPointer, Index.NameMapSize));
            for (var i = 0; i < Index.FileCount; ++i)
            {
                var offset = MemoryMarshal.Read<int>(data.Slice(Header.FileTablePointer + i * 8));
                var length = MemoryMarshal.Read<int>(data.Slice(Header.FileTablePointer + i * 8 + 4));
                Blobs.Add(new Memory<byte>(data.Slice(Header.FileTablePointer + offset, length).ToArray()));
            }
        }

        /// <summary>
        ///     GMPK Header
        /// </summary>
        public GMPKHeader Header { get; set; }

        /// <summary>
        ///     GMPK Index
        /// </summary>
        public GMPKIndex Index { get; set; }

        /// <summary>
        ///     GMPK Data Blobs
        /// </summary>
        public List<Memory<byte>> Blobs { get; set; } = new List<Memory<byte>>();

        /// <summary>
        ///     Name Map for the files stored within
        /// </summary>
        public GPKNameMap NameMap { get; set; }
    }
}
