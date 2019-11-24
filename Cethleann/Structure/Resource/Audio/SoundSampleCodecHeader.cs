namespace Cethleann.Structure.Resource.Audio
{
#pragma warning disable 1591
    public struct SoundSampleCodecHeader
    {
        public SoundSampleCodec Codec { get; set; }
        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public byte Unknown3 { get; set; }
        public int HeaderSize { get; set; }
        public byte LayerCount { get; set; }
        public byte ChannelCount { get; set; }
        public short Unknown4 { get; set; }
        public int SampleRate { get; set; }
        public int SampleCount { get; set; }
        public int LoopStart { get; set; }
        public int LoopLength { get; set; }
        public int Reserved1 { get; set; }
        public int AudioOffset { get; set; }
        public int AudioSize { get; set; }
        public int Reserved2 { get; set; }
        public int FrameCount { get; set; }
        public short FrameSize { get; set; }
        public short SamplesPerFrame { get; set; }
        public int InputSampleRate { get; set; }
        public short Skip { get; set; }
        public byte StreamCount { get; set; }
        public byte CoupleCount { get; set; }
    }
#pragma warning restore 1591
}
