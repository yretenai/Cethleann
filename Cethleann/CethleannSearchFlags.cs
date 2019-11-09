using System;

namespace Cethleann
{
    [Flags]
    public enum CethleannSearchFlags : uint
    {
        None = 0,
        Base = 0x1,

        AllDLC = 0xFFF << 4,

        DLC1 = 0x001 << 4,
        DLC2 = 0x002 << 4,
        DLC3 = 0x004 << 4,
        DLC4 = 0x008 << 4,
        DLC5 = 0x010 << 4,
        DLC6 = 0x020 << 4,

        Patch1 = 0x1 << 16,
        Patch2 = 0x2 << 16,
        Patch3 = 0x4 << 16,

        AllPatch = 0xFFFFu << 16,

        All = 0xFFFFFFFF
    }
}
