namespace Cethleann.Structure.Resource.Audio.WHD
{
    public struct KWB2Entry
    {
        public ushort Version { get; set; }
        public byte Unknown3 { get; set; }
        public byte Streams { get; set; }
        public int Unknown4 { get; set; }
        public float Unknown5 { get; set; }
        public float Unknown6 { get; set; }
        public float Unknown7 { get; set; }
        public int Unknown8 { get; set; }
        public int Unknown9 { get; set; }
        public int Unknown10 { get; set; }
        public int Unknown11 { get; set; }
        public int Unknown12 { get; set; }
        public int Unknown13 { get; set; }
        public short BlockOffset { get; set; }
        public short BlockSize { get; set; }
        public int Unknown16 { get; set; }
    }
}
