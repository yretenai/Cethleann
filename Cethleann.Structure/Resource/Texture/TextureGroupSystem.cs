using JetBrains.Annotations;

namespace Cethleann.Structure.Resource.Texture
{
    // Could also be version, unlikely though.
    // TODO: Compare Atelier Ryza PC and Switch
    [PublicAPI]
    public enum TextureGroupSystem : uint
    {
        // Or WiiU?
        Playstation3 = 0x1,
        ThreeDS = 0x5,
        Vita = 0x6,
        Mobile = 0x7,

        // Assumption
        XboxOne = 0x9,
        Windows = 0xA,

        // Also bad PC ports
        Playstation4 = 0xB,
        Switch = 0x10
    }
}
