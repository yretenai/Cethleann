namespace Cethleann.Structure
{
    public struct GPKNameMapHeader
    {
        public uint Magic { get; set; }
        public int Size { get; set; }
        public int Count { get; set; }
        public int Unknown { get; set; }
    }
}
