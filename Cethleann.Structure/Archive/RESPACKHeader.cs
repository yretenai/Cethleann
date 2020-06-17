namespace Cethleann.Structure.Archive
{
    public struct RESPACKHeader
    {
        public DataType Magic { get; set; }
        public int LongMagic { get; set; }
        public int Version { get; set; }
        public int HeaderSize { get; set; }
        public int FileSize { get; set; }
        public int PointerCount { get; set; }
        public int UnknownCount { get; set; }
        public int Unknown { get; set; }
        public int PointerTablePointer { get; set; }
        public int SizeTablePointer { get; set; }
        public int ExtraPointer { get; set; }
    }
}
