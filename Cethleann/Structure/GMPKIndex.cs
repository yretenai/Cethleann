namespace Cethleann.Structure
{
#pragma warning disable 1591
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
#pragma warning restore 1591
}
