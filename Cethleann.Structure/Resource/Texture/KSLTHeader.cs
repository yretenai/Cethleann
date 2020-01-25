namespace Cethleann.Structure.Resource.Texture
{
public struct KSLTHeader
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public int Count { get; set; }
        public int Size { get; set; }
        public int PointerTablePointer { get; set; }
        public int NameTableSize { get; set; }
        public int NameCount { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        public int Unknown5 { get; set; }
        public int Unknown6 { get; set; }
        public int Unknown7 { get; set; }
        public int Unknown8 { get; set; }
        public int Unknown9 { get; set; }
    }
}
