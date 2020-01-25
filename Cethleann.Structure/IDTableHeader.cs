namespace Cethleann.Structure
{
    public struct IDTableHeader
    {
        public int Offset { get; set; }
        public int Count { get; set; }
        public uint Block { get; set; }
        public uint Padding { get; set; }
    }
}
