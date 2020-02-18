using System;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Ninja
{
    [PublicAPI]
    public class BTIF
    {
        public BTIF(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<BTIFHeader>(buffer);
            Entries = MemoryMarshal.Cast<byte, BTIFEntry>(buffer.Slice(SizeHelper.SizeOf<BTIFHeader>(), Header.BlockSize)).ToArray();
            UnknownBuffer = buffer.Slice(SizeHelper.SizeOf<BTIFHeader>() + Header.MangledPointer, 0x80).ToArray();
        }

        public BTIFHeader Header { get; set; }
        public BTIFEntry[] Entries { get; set; }
        public byte[] UnknownBuffer { get; set; }
    }
}
