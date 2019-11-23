namespace Cethleann.Koei.Structure.Resource.Model
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct ModelGeometryTextureSet
    {
        public short Index { get; set; }
        public short TexCoord { get; set; }
        public TextureKind Kind { get; set; }
        public TextureKind AlternateKind { get; set; }
        public short Unknown4 { get; set; }
        public short Unknown5 { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
