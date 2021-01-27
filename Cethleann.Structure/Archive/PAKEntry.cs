namespace Cethleann.Structure.Archive
{
    public struct PAKEntry
    {
        public string Filename { get; set; }
        public int Size { get; set; }
        public byte[] Key { get; set; }
        public ulong Offset { get; set; }
        public ulong Flags { get; set; }
        public bool IsEncrypted { get; set; }
    }
}
