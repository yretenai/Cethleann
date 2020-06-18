namespace Cethleann.Structure.Table
{
    public struct ECBStringHeader
    {
        public uint Hash { get; set; }
        public ECBCharSet CharSet { get; set; }
        public short Size { get; set; }
    }
}
