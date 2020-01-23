namespace Cethleann.Structure.Resource.Audio
{
#pragma warning disable 1591
    public struct SoundContainerHeader
    {
        public DataType Magic { get; set; }
        public short ConsoleType { get; set; }
        public GameId GameId { get; set; }
        public int Count { get; set; }
        public int IdTablePointer { get; set; }
        public int PointerTablePointer { get; set; }
        public int FileSize { get; set; }
        public int ResourcePointer { get; set; }
    }
#pragma warning restore 1591
}
