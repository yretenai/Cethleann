using System;

namespace Cethleann
{
    /// <summary>
    ///     Flags used to optimize <seealso cref="Cethleann.ReadEntry" />
    /// </summary>
    [Flags]
    public enum CethleannSearchFlags : uint
    {
        /// <summary>
        ///     Don't read from any container
        /// </summary>
        None = 0,

        /// <summary>
        ///     Read from the base game
        /// </summary>
        Base = 0x1,

        /// <summary>
        ///     Read from all DLC
        /// </summary>
        AllDLC = 0xFFF << 4,

        /// <summary>
        ///     Read from DLC1
        /// </summary>
        DLC1 = 0x001 << 4,

        /// <summary>
        ///     Read from DLC2
        /// </summary>
        DLC2 = 0x002 << 4,

        /// <summary>
        ///     Read from DLC3
        /// </summary>
        DLC3 = 0x004 << 4,

        /// <summary>
        ///     Read from DLC4
        /// </summary>
        DLC4 = 0x008 << 4,

        /// <summary>
        ///     Read from DLC5
        /// </summary>
        DLC5 = 0x010 << 4,

        /// <summary>
        ///     Read from DLC6
        /// </summary>
        DLC6 = 0x020 << 4,

        /// <summary>
        ///     Read from all patches
        /// </summary>
        AllPatch = 0xFFFFu << 16,

        /// <summary>
        ///     Read from Patch1
        /// </summary>
        Patch1 = 0x1 << 16,

        /// <summary>
        ///     Read from Patch2
        /// </summary>
        Patch2 = 0x2 << 16,

        /// <summary>
        ///     Read from Patch3
        /// </summary>
        Patch3 = 0x4 << 16,

        /// <summary>
        ///     Read all files
        /// </summary>
        All = 0xFFFFFFFF
    }
}
