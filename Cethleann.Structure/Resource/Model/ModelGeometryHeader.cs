using DragonLib.Numerics;

namespace Cethleann.Structure.Resource.Model
{
    public struct ModelGeometryHeader
    {
        public int ModelType { get; set; }
        public float BoundingBoxWeight { get; set; }
        public Vector3 BoundingBoxBottomRight { get; set; }
        public Vector3 BoundingBoxTopLeft { get; set; }
        public int Count { get; set; } // assumption.
    }
}
