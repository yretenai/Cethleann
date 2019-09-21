using System.Runtime.InteropServices;

namespace Cethleann.Structure.Math
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0x10)]
    public struct Vector4f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
