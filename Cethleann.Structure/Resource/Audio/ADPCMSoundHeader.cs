namespace Cethleann.Structure.Resource.Audio
{
    public struct ADPCMSoundHeader
    {
        public SoundResourceEntry Base { get; set; }
        public int Streams { get; set; }
        public int FrameSize { get; set; }
        public int SampleRate { get; set; }
        public int Nibbles { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int ADPCMPointer { get; set; }
        public int ADPCMSize { get; set; }
        public int PointerTablePointer { get; set; }
        public int SizeTablePointer { get; set; }
    }
}
