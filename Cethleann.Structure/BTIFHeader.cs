namespace Cethleann.Structure
{
    public struct BTIFHeader
    {
        public DataType Magic { get; set; }
        public int Unknown1 { get; set; }
        public int EntryCount { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        public int BlockSize { get; set; }
        public int TextPointer { get; set; }
        public int Unknown5 { get; set; }
        public int MangledPointer { get; set; }
    }
}
