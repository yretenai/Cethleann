namespace Cethleann.Structure.WHD
{
#pragma warning disable 1591
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
#pragma warning restore 1591
}
