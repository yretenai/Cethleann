namespace Cethleann.Structure.Art
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct ModelSkeletonHeader
    {
        public int DataOffset { get; set; }
        public int SkeletonCount { get; set; }
        public short BoneCount { get; set; }
        public short BoneTableCount { get; set; }
        public int Unknown1 { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
