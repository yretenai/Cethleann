using DragonLib;

namespace Cethleann.Structure.Resource.Animation
{
    public struct AnimationV2BoneInfo
    {
        [BitField(2)]
        public byte SplineCount { get; set; }

        [BitField(10)]
        public short BoneId { get; set; }

        [BitField(20)]
        public int DataOffset { get; set; }
    }
}
