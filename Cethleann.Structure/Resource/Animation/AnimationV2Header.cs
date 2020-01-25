namespace Cethleann.Structure.Resource.Animation
{
    public struct AnimationV2Header
    {
        public float Framerate { get; set; }
        public uint PackedInfo { get; set; }
        public int TimingSectionSize { get; set; }
        public int QuantizedDataCount { get; set; }
    }
}
