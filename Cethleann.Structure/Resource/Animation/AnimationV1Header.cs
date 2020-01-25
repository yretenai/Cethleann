namespace Cethleann.Structure.Resource.Animation
{
    public struct AnimationV1Header
    {
        public short Unknown1 { get; set; }
        public short Unknown2 { get; set; }
        public float Duration { get; set; }
        public int SplinePointer { get; set; }
        public int Reserved1 { get; set; }
        public int Reserved2 { get; set; }
        public int Reserved3 { get; set; }
        public int Reserved4 { get; set; }
        public int Reserved5 { get; set; }
        public int Reserved6 { get; set; }
        public short Count { get; set; }
        public short MaxBoneId { get; set; }
    }
}
