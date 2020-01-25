using DragonLib;

namespace Cethleann.Structure.Resource.Texture
{
    public struct TexturePackedSize
    {
        [BitField(4)]
        public byte Width { get; set; }
        
        [BitField(4)]
        public byte Height { get; set; }
    }
}
