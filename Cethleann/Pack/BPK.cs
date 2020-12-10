using Cethleann.Structure.Archive;
using DragonLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cethleann.Pack
{
    /// <summary>
    ///     Binary Pack
    /// </summary>
    [PublicAPI]
    public class BPK
    {
        /// <summary>
        ///     Initialize with a buffer
        /// </summary>
        /// <param name="buffer"></param>
        public BPK(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<BPKHeader>(buffer);
            if (Header.PointerCount == 0) return;
            Pointers = MemoryMarshal.Cast<byte, int>(buffer.Slice(SizeHelper.SizeOf<BPKHeader>(), Header.PointerCount * 4)).ToArray();
            var sizes = new int[Header.PointerCount];
            for (var i = 0; i < Header.PointerCount; ++i)
                sizes[i] = (i < Header.PointerCount - 1 ? Pointers[i + 1] : buffer.Length) - Pointers[i];

            for (var i = 0; i < Header.PointerCount; ++i) Entries.Add(sizes[i] > 0 ? new Memory<byte>(buffer.Slice(Pointers[i], sizes[i]).ToArray()) : Memory<byte>.Empty);
        }

        /// <summary>
        ///     List of pointers defined by the BPK
        /// </summary>
        public int[] Pointers { get; set; } = Array.Empty<int>();

        /// <summary>
        ///     Underlying Header
        /// </summary>
        public BPKHeader Header { get; set; }

        /// <summary>
        ///     Data Streams found in the BPK
        /// </summary>
        public List<Memory<byte>> Entries { get; set; } = new List<Memory<byte>>();
    }
}
