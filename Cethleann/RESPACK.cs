using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure;

namespace Cethleann
{
    /// <summary>
    ///     Packed Resource files
    /// </summary>
    public class RESPACK
    {
        /// <summary>
        ///     Initialize with a buffer
        /// </summary>
        /// <param name="buffer"></param>
        public RESPACK(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<RESPACKHeader>(buffer);
            if (Header.PointerCount == 0) return;
            Pointers = MemoryMarshal.Cast<byte, int>(buffer.Slice(Header.PointerTablePointer, Header.PointerCount * 4)).ToArray();
            var sizes = new int[Header.PointerCount];
            if (Header.SizeTablePointer > 0)
                MemoryMarshal.Cast<byte, int>(buffer.Slice(Header.SizeTablePointer, Header.PointerCount * 4));
            else
                for (var i = 0; i < Header.PointerCount; ++i)
                    sizes[i] = (i < Header.PointerCount - 1 ? Pointers[i + 1] : buffer.Length) - Pointers[i];

            for (var i = 0; i < Header.PointerCount; ++i) Entries.Add(sizes[i] > 0 ? new Memory<byte>(buffer.Slice(Pointers[i], sizes[i]).ToArray()) : Memory<byte>.Empty);
        }

        /// <summary>
        ///     List of pointers defined by the RESPACK
        /// </summary>
        public int[] Pointers { get; set; }

        /// <summary>
        ///     Underlying Header
        /// </summary>
        public RESPACKHeader Header { get; set; }

        /// <summary>
        ///     Data Streams found in the PAK
        /// </summary>
        public List<Memory<byte>> Entries { get; set; } = new List<Memory<byte>>();
    }
}
