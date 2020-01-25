namespace Cethleann.Structure.Resource.Audio
{
    public struct SoundContainerHeader
    {
        public DataType Magic { get; set; }
        public short ConsoleType { get; set; }
        public short UnknownFlag { get; set; }
        public int Count { get; set; }
        public int IdTablePointer { get; set; }
        public int PointerTablePointer { get; set; }
        public int FileSize { get; set; }
        public int ResourcePointer { get; set; }
    }
}
