using DragonLib.Numerics;
using OpenTK.Mathematics;
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
            var matrixR = Matrix4.CreateFromQuaternion(new OpenTK.Mathematics.Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W).Inverted());
            var matrixS = Matrix4.CreateScale(Scale.X, Scale.Y, Scale.Z);
            var matrix = matrixT * matrixR * matrixS;
            return new Matrix4x4(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }
    }
}
