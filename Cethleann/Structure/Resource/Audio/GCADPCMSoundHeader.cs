namespace Cethleann.Structure.Resource.Audio
{
#pragma warning disable 1591
    public struct GCADPCMSoundHeader
    {
        public SoundResourceEntry Base { get; set; }
        public int Channels { get; set; }
        public int FrameSize { get; set; }
        public int SampleRate1 { get; set; }
        public int SamplesAudible1 { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int ADPCMPointer { get; set; }
        public int Unknown4 { get; set; }
        public int UnknownPointer { get; set; }
        public int AudioPointer { get; set; }
        public int SamplesAudible2 { get; set; }
        public int Samples { get; set; }
        public int SampleRate2 { get; set; }
        public int Unknown5 { get; set; }
        public int Unknown6 { get; set; }
        public int SampleRate3 { get; set; }
        public int Unknown7 { get; set; }
    }
#pragma warning restore 1591
}
