namespace Cethleann.Structure.Resource.Animation
{
    public class AnimationV2Header
    {
        public float Framerate { get; set; }
        public int PackedInfo { get; set; }
        public int TimingSectionSize { get; set; }
        public int BoneCount { get; set; }
    }
}
