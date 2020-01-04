using System;

namespace Cethleann.Structure
{
    [Flags]
#pragma warning disable 1591
    public enum IDTableFlags : uint
    {
        None = 0,
        Compressed = 0x40000000,
        Encrypted = 0x80000000
    }
#pragma warning restore 1591
}
