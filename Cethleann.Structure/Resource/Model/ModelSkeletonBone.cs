using DragonLib;
using DragonLib.Numerics;
using OpenTK;
using Quaternion = DragonLib.Numerics.Quaternion;
using Vector3 = DragonLib.Numerics.Vector3;

namespace Cethleann.Structure.Resource.Model
{
    public struct ModelSkeletonBone
    {
        public Vector3 Scale { get; set; }
        public int Parent { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Position { get; set; }
        public float Length { get; set; }

        public bool HasParent() => Parent > -1;

        public Matrix4x4 GetMatrix()
        {
            var matrixT = Matrix4.CreateTranslation(0 - Position.X, 0 - Position.Y, 0 - Position.Z);
            var matrixR = Matrix4.CreateFromQuaternion(Rotation.ToOpenTK().Inverted());
            var matrixS = Matrix4.CreateScale(Scale.X, Scale.Y, Scale.Z);
            return (matrixT * matrixR * matrixS).ToDragon();
        }
    }
}
