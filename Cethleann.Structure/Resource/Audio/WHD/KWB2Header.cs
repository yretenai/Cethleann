namespace Cethleann.Structure.Resource.Audio.WHD
{
    public struct KWB2Header
    {
        public WBHSoundbankType Magic { get; set; }
        public short Unknown1 { get; set; }
        public short Count { get; set; }
        public int Unknown2 { get; set; }
        public float Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        public int HDDBPointer { get; set; }
    }
}
