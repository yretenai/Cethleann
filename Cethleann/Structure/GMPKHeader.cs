namespace Cethleann.Structure
{
#pragma warning disable 1591
    public struct GMPKHeader
    {
        public DataType Magic;
        public uint Version;
        public uint System;
        public int FileTablePointer { get; set; }
        public int Unknown1Count { get; set; }
        public int Unknown2Count { get; set; }
        public int Unknown3Count { get; set; }
        public int Unknown4Count { get; set; }
        public int Unknown1Pointer { get; set; }
        public int NameMapIndexPointer { get; set; }
        public int Unknown1 { get; set; }
        public int EntryMapPointer { get; set; }
    }

#pragma warning restore 1591
}
