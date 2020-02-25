using System;

namespace Cethleann.Structure.Archive
{
    [Flags]
    public enum IDTableFlags : uint
    {
        None = 0,
        Streamed = 0x20000000,
        Compressed = 0x40000000,
        Encrypted = 0x80000000
    }
}
