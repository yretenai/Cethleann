namespace Cethleann.Structure.Archive
{
    public struct LFMOrderEntry
    {
        public int Reserved { get; set; }
        public int FileId { get; set; }
        public int Pointer { get; set; }
    }
}
