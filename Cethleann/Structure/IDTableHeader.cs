namespace Cethleann.Structure
{
#pragma warning disable 1591
    public struct IDTableHeader
    {
        public int Offset { get; set; }
        public int Count { get; set; }
        public uint Block { get; set; }
        public uint Padding { get; set; }
    }
#pragma warning restore 1591
}
