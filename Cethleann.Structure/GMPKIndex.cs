namespace Cethleann.Structure
{
    public struct GMPKIndex
    {
        public int EntryMapPointer { get; set; }
        public int NameMapPointer { get; set; }
        public int NameMapSize { get; set; }
        public int Unknown1 { get; set; }
        public int FileCount { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
    }
}
