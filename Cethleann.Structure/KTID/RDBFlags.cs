using System;

namespace Cethleann.Structure.KTID
{
    [Flags]
    public enum RDBFlags
    {
        External = 0x00010000,
        Internal = 0x00020000,
        ZlibCompressed = 0x00100000,
        Lz4Compressed = 0x00200000,
        Encrypted = 0x00200000, // reused in p5s pc
    }
}
