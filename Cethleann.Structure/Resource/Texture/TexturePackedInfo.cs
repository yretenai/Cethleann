using DragonLib;

namespace Cethleann.Structure.Resource.Texture
{
    public struct TexturePackedInfo
    {
        [BitField(4)]
        public byte Unknown { get; set; }

        [BitField(4)]
        public byte Mips { get; set; }
    }
}
