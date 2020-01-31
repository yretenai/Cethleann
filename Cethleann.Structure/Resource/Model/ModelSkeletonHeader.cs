namespace Cethleann.Structure.Resource.Model
{
    public struct ModelSkeletonHeader
    {
        public int DataOffset { get; set; }
        public int Layer { get; set; }
        public short BoneCount { get; set; }
        public short BoneTableCount { get; set; }
        public int Unknown1 { get; set; }
    }
}
