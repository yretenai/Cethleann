using DragonLib.Numerics;

namespace Cethleann.Koei.Structure.Resource.Model
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct ModelGeometryHeader
    {
        public int ModelType { get; set; }
        public float BoundingBoxWeight { get; set; }
        public Vector3 BoundingBoxBottomRight { get; set; }
        public Vector3 BoundingBoxTopLeft { get; set; }
        public int Count { get; set; } // assumption.
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
