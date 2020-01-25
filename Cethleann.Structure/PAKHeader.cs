namespace Cethleann.Structure
{
    public struct PAKHeader
    {
        public int Version { get; set; }
        public int FileCount { get; set; }
        public int HeaderSize { get; set; }
        public int Flags { get; set; }
    }
}
