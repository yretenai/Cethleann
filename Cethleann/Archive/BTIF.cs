using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Archive;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Archive
{
    /// <summary>
    ///     PKGINFO BTIF section parser.
    /// </summary>
    [PublicAPI]
    public class BTIF
    {
        /// <summary>
        ///     initialize with stream data
        /// </summary>
        /// <param name="buffer"></param>
        public BTIF(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<BTIFHeader>(buffer);
            Entries = MemoryMarshal.Cast<byte, BTIFEntry>(buffer.Slice(SizeHelper.SizeOf<BTIFHeader>(), Header.BlockSize)).ToArray();
            UnknownBuffer = buffer.Slice(SizeHelper.SizeOf<BTIFHeader>() + Header.MangledPointer, 0x80).ToArray();
        }

        /// <summary>
        ///     BTIF Header
        /// </summary>
        public BTIFHeader Header { get; set; }

        /// <summary>
        ///     lsof file entries
        /// </summary>
        public BTIFEntry[] Entries { get; set; }

        /// <summary>
        ///     Unknown byte buffer at the end of the stream
        /// </summary>
        public byte[] UnknownBuffer { get; set; }
    }
}
