using DragonLib;

namespace Cethleann.Structure.Resource.Model
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct ModelSkeletonBone
    {
        public Vector Scale { get; set; }
        public uint Parent { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector Position { get; set; }
        public float Length { get; set; }

        public bool HasParent() => Parent < 0xFFFF_FFFF;
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
