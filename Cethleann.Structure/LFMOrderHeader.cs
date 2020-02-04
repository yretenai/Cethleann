namespace Cethleann.Structure
{
    public struct LFMOrderHeader
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public int Count { get; set; }
        public int LargestFilenameLength { get; set; }
        public int DataPointer { get; set; }
        public int NameTablePointer { get; set; }
        public int FirstNamePointer { get; set; }
        public int Reserved1 { get; set; }
        public int Reserved2 { get; set; }
        public int SourceNamePointer { get; set; }
    }
}
