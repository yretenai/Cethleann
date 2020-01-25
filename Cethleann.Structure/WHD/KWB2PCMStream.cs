namespace Cethleann.Structure.WHD
{
    public struct KWB2PCMStream
    {
        public ushort SampleRate { get; set; }
        public KWB2PCMCodec Codec { get; set; }
        public byte Channels { get; set; }
        public short FrameSize { get; set; }
        public short SamplesPerFrame { get; set; }
        public int Unknown4 { get; set; }
        public int SampleCount { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }
    }
}
