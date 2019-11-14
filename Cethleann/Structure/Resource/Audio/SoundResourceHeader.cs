namespace Cethleann.Structure.Resource.Audio
{
#pragma warning disable 1591
    public struct SoundResourceHeader
    {
        public DataType Magic { get; set; }
        public SoundResourceSectionType SectionType { get; set; }
        public SoundResourceFlags Flags { get; set; }
        public short ConsoleType { get; set; }
        public SoundResourceGame GameId { get; set; }
        public long Reserved { get; set; }
        public int Size { get; set; }
        public int CompressedSize { get; set; }
    }
#pragma warning restore 1591
}
