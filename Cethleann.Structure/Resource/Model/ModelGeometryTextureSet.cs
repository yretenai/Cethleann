namespace Cethleann.Structure.Resource.Model
{
    public struct ModelGeometryTextureSet
    {
        public short Index { get; set; }
        public short TexCoord { get; set; }
        public TextureKind Kind { get; set; }
        public TextureKind AlternateKind { get; set; }
        public short Unknown4 { get; set; }
        public short Unknown5 { get; set; }
    }
}
