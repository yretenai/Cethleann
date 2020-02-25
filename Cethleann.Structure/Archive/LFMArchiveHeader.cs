namespace Cethleann.Structure.Archive
{
    public struct LFMArchiveHeader
    {
        public DataType Magic { get; set; }
        public int Zero { get; set; }
        public long Files { get; set; }
        public long FileSize { get; set; }
        public long Align { get; set; }
    }
}
