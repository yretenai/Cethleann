namespace Cethleann.Structure
{
#pragma warning disable 1591
    public struct SCENHeader
    {
        public DataType Magic { get; set; }
        public int Unknown1 { get; set; }
        public short Unknown2 { get; set; }
        public byte Unknown3 { get; set; }
        public byte Unknown4 { get; set; }
        public int HeaderSize { get; set; }
        public int FileSize { get; set; }
        public int OffsetCount { get; set; }
        public int SizeCount { get; set; }
        public int UnknownCount { get; set; }
        public int OffsetTableOffset { get; set; }
        public int SizeTableOffset { get; set; }
        public int UnknownTableOffset { get; set; }
        public int Unknown5 { get; set; }
    }
#pragma warning restore 1591
}
