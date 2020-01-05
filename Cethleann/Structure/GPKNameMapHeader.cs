namespace Cethleann.Structure
{
#pragma warning disable 1591
    public struct GPKNameMapHeader
    {
        public ulong Magic { get; set; }
        public int Version { get; set; }
        public int Size { get; set; }
        public int Count { get; set; } 
        public int Unknown { get; set; } 
    }
#pragma warning restore 1591
}
