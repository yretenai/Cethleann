namespace Cethleann.Structure.Archive
{
    public struct PAKEntry
    {
        public string Filename { get; set; }
        public int Size { get; set; }
        public byte[] Key { get; set; }
        public long Offset { get; set; }
        public long Flags { get; set; }
        public uint Unknown { get; set; }
        public bool IsEncrypted { get; set; }
    }
}
