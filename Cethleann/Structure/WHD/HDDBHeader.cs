namespace Cethleann.Structure.WHD
{
#pragma warning disable 1591
    public struct HDDBHeader
    {
        public DataType Magic { get; set; }
        public int Unknown1 { get; set; }
        public int EntryTablePointer { get; set; }
        public int EntrySize { get; set; }
        public int Size { get; set; }
        public int NameTablePointer { get; set; }
        public int Unknown5 { get; set; }
        public int Unknown6 { get; set; }
    }
#pragma warning restore 1591
}
