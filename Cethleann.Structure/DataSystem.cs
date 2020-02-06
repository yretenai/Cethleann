using JetBrains.Annotations;

namespace Cethleann.Structure
{
    [PublicAPI]
    public enum DataSystem : uint
    {
        Prehistoric = 0x0,
        
        // Special Tiled Z Morton Swizzling
        PS3 = 0x1,
        
        // Extra Special Tiling Swizzling
        X360 = 0x2,
        
        // Assumption, I could not find G1Ts with this value but it fits the timeline.
        // Could also be DX9
        NWii = 0x3,
        
        // Assumption, I could not find G1Ts with this value but it fits the timeline.
        // Could also be DX9
        NDS = 0x4,
        N3DS = 0x5,
        PSVita = 0x6,
        
        // ETC LOL
        Android = 0x7,
        
        // Lord knows what formats lie here
        iOS = 0x8,
        
        // Big Endian LOL
        NWiiU = 0x9,
        
        // Could also be DX11.
        Windows = 0xA,

        // Special Z Morton Swizzling
        PS4 = 0xB,
        XOne = 0xC,

        // Assumption, I could not find G1Ts with this value.
        // I'm assuming this is for DoA5 Arcade? Could be DX12.
        Arcade = 0xD,

        // Assumption, I could not find G1Ts with this value but it fits the timeline.
        // Could also be DFFNT Arcade, or DX12
        PS5 = 0xE,

        // Assumption, I could not find G1Ts with this value but it fits the timeline.
        // Could also be DoA6 Arcade, or DX12
        XScarlett = 0xF,
        Switch = 0x10
    }
}
