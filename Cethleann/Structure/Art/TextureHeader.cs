namespace Cethleann.Structure.Art
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct TextureHeader
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public int Size { get; set; }
        public int TableOffset { get; set; }
        public int EntrySize { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
