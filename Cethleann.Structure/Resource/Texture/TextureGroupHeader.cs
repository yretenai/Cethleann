namespace Cethleann.Structure.Resource.Texture
{
    public struct TextureGroupHeader
    {
        public int TableOffset { get; set; }
        public int Count { get; set; }
        public DataSystem System { get; set; }
        public int Unknown2 { get; set; }
    }
}
