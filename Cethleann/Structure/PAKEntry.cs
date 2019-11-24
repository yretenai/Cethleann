namespace Cethleann.Structure
{
#pragma warning disable 1591
    public struct PAKEntry
    {
        public string Filename { get; set; }
        public int Size { get; set; }
        public byte[] Key { get; set; }
        public long Offset { get; set; }
        public long Flags { get; set; }
        public bool IsEncrypted { get; set; }
    }
#pragma warning disable 1591
}
