using JetBrains.Annotations;

namespace Cethleann.Structure.Resource.Texture
{
    // Could also be version, unlikely though.
    // TODO: Compare Atelier Ryza PC and Switch
    [PublicAPI]
    public enum TextureGroupSystem : uint
    {
        PS2 = 0x0,
        PS3 = 0x1,
        X360 = 0x2,
        NWii = 0x3,
        NDS = 0x4,
        N3DS = 0x5,
        PSVita = 0x6,
        Android = 0x7,
        iOS = 0x8,
        NWiiU = 0x9,
        Windows = 0xA,

        // Also bad PC ports
        PS4 = 0xB,
        XOne = 0xC,
        
        // Assumption
        Arcade = 0xD,
        
        // Assumption
        PS5 = 0xE,
        
        // Assumption
        XScarlett = 0xF,
        Switch = 0x10
    }
}
