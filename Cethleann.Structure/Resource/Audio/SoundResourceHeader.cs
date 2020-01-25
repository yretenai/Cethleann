namespace Cethleann.Structure.Resource.Audio
{
    public struct SoundResourceHeader
    {
        public DataType Magic { get; set; }
        public SoundResourceSectionType SectionType { get; set; }
        public SoundResourceFlags Flags { get; set; }
        public short ConsoleType { get; set; }
        public GameId GameId { get; set; }
        public long Reserved { get; set; }
        public int Size { get; set; }
        public int CompressedSize { get; set; }
    }
}
