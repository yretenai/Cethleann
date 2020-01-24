using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure;

namespace Cethleann
{
    /// <summary>
    ///     RPRPK Packed files
    /// </summary>
    public class RTRPK
    {
        /// <summary>
        ///     Initialize with a buffer
        /// </summary>
        /// <param name="buffer"></param>
        public RTRPK(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<RTRPKHeader>(buffer);
            var pointers = MemoryMarshal.Cast<byte, int>(buffer.Slice(Header.PointerTablePointer, Header.PointerCount * 4));
            var sizes = MemoryMarshal.Cast<byte, int>(buffer.Slice(Header.SizeTablePointer, Header.SizeCount * 4));
            for (var i = 0; i < Header.PointerCount; ++i) Entries.Add(new Memory<byte>(buffer.Slice(pointers[i], sizes[i]).ToArray()));
        }

        /// <summary>
        ///     Underlying Header
        /// </summary>
        public RTRPKHeader Header { get; set; }

        /// <summary>
        ///     Data Streams found in the PAK
        /// </summary>
        public List<Memory<byte>> Entries { get; set; } = new List<Memory<byte>>();
    }
}
