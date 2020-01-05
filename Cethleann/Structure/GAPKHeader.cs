namespace Cethleann.Structure
{
#pragma warning disable 1591
    public struct GAPKHeader
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public short Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public byte Unknown3 { get; set; }
        public int HeaderSize { get; set; }
        public int FileSize { get; set; }
        public int PointerTableCount { get; set; }
        public int SizeTableCount { get; set; }
        public int Unknown4 { get; set; }
        public int PointerTablePointer { get; set; }
        public int SizeTablePointer { get; set; }
        public int IndexTablePointer { get; set; }
    }

#pragma warning restore 1591
}
