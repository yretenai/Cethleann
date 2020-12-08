namespace Cethleann.Structure.KTID
{
    public struct PRDBHeader
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public int HeaderSize { get; set; }
        public int FileSize { get; set; }
    }
}
