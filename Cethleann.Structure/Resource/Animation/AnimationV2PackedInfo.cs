using DragonLib;

namespace Cethleann.Structure.Resource.Animation
{
    public struct AnimationV2PackedInfo
    {
        [BitField(18)]
        public int AnimationLength { get; set; }

        [BitField(14)]
        public int BlobSize { get; set; }
    }
}
