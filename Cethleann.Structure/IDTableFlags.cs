using System;

namespace Cethleann.Structure
{
    [Flags]
    public enum IDTableFlags : uint
    {
        None = 0,
        Compressed = 0x40000000,
        Encrypted = 0x80000000
    }
}
