namespace Cethleann.Structure.Resource.Audio
{
#pragma warning disable 1591
    public struct GCADPCMSoundHeader
    {
        public SoundResourceEntry Base { get; set; }
        public int Streams { get; set; }
        public int FrameSize { get; set; }
        public int MaxSampleRate { get; set; }
        public int Nibbles { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int ADPCMPointer { get; set; }
        public int ADPCMSize { get; set; }
        public int AudioTablePointer { get; set; }
        public int UnknownPointer { get; set; }
    }
#pragma warning restore 1591
}
