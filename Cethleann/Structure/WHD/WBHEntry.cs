namespace Cethleann.Structure.WHD
{
#pragma warning disable 1591
    public struct WBHEntry
    {
        public int Offset { get; set; }
        public int Size { get; set; }
        public int Samples { get; set; }
        public WBHCodec Codec { get; set; }
        public int Frequency { get; set; }
        public short BlockAlign { get; set; }
    }
#pragma warning restore 1591
}
