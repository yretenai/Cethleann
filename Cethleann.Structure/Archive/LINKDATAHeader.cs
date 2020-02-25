namespace Cethleann.Structure.Archive
{
    public struct LINKDATAHeader
    {
        public DataType DataType { get; set; }
        public int EntryCount { get; set; }
        public int OffsetMultiplier { get; set; }
        public int Reserved { get; set; }
    }
}
