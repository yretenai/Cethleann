namespace Cethleann.Structure.Resource.Audio
{
    public struct GCADPCMSoundInfo
    {
        public int Nibbles { get; set; }
        public int SampleCount { get; set; }
        public int SampleRate { get; set; }
        public int Unknown1 { get; set; } // interleave state?
        public int Unknown2 { get; set; }
        public int CutSampleRate { get; set; }
        public int Unknown3 { get; set; }
        public GCADPCMCoefficient Coefficient1 { get; set; }
        public GCADPCMCoefficient Coefficient2 { get; set; }
        public int Unknown4 { get; set; }
        public int Unknown5 { get; set; }
        public int Unknown6 { get; set; }
        public int Unknown7 { get; set; }
        public int Unknown8 { get; set; }
        public int Unknown9 { get; set; }
        public int Unknown10 { get; set; }
        public int Unknown11 { get; set; }
        public int Unknown12 { get; set; }
    }
}
