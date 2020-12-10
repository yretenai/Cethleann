namespace Cethleann.Structure.Archive
{
    public struct BPKHeader
    {
        public DataType Magic { get; set; }
        public int PointerCount { get; set; }
        public int Width { get; set; }
    }
}
