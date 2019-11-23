namespace Cethleann.Koei.Structure.Resource
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct ResourceSectionHeader
    {
        public ResourceSection Magic { get; set; }
        public int Version { get; set; }
        public int Size { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
