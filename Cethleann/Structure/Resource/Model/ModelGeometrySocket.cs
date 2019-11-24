using DragonLib.Numerics;

namespace Cethleann.Structure.Resource.Model
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct ModelGeometrySocket
    {
        public int BoneId { get; set; }
        public float Weight { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Position { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
