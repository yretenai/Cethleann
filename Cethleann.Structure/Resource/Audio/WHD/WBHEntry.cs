namespace Cethleann.Structure.Resource.Audio.WHD
{
    public struct WBHEntry
    {
        public int Offset { get; set; }
        public int Size { get; set; }
        public int Samples { get; set; }
        public short Channels { get; set; }
        public WAVECodec Codec { get; set; }
        public int Frequency { get; set; }
        public short BlockAlign { get; set; }
        public object Setup { get; set; }
    }
}
