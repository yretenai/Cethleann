namespace Cethleann.Structure.Resource.Audio
{
    public struct KOVSHeader
    {
        public DataType Magic { get; set; }
        public int Size { get; set; }
        public int LoopStartSamples { get; set; }
        public int LoopEndSamples { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
    }
}
