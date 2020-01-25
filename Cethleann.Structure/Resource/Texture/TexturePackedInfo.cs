using DragonLib;

namespace Cethleann.Structure.Resource.Texture
{
    public struct TexturePackedInfo
    {
        [BitField(4)]
        public byte System { get; set; }
        
        [BitField(4)]
        public byte Mips { get; set; }
    }
}
