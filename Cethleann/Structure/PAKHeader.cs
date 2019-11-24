namespace Cethleann.Structure
{
#pragma warning disable 1591
    public struct PAKHeader
    {
        public int Version { get; set; }
        public int FileCount { get; set; }
        public int HeaderSize { get; set; }
        public int Flags { get; set; }
    }
#pragma warning restore 1591
}
