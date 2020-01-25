using DragonLib.Numerics;

namespace Cethleann.Structure.Resource.Model
{
public struct ModelGeometrySocket
    {
        public int BoneId { get; set; }
        public float Weight { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Position { get; set; }
    }
}
