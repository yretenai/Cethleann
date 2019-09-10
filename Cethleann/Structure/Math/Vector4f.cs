using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Cethleann.Structure.Math
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0x10)]
    public struct Vector4f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
    }
}
