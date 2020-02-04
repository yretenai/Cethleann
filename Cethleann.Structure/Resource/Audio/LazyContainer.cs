namespace Cethleann.Structure.Resource.Audio
{
    public struct LazyContainer
    {
        public ResourceSectionHeader Header { get; set; }
        public int DataStartPointer { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Pointer { get; set; }
    }
}
