namespace Cethleann.Structure.Table
{
    public struct ECBHeader
    {
        public DataType Magic { get; set; }
        public int FieldCount { get; set; }
        public int EntryCount { get; set; }
        public int Stride { get; set; }
        public int DynamicDataPointer { get; set; }
        public int TotalSize { get; set; }
    }
}
