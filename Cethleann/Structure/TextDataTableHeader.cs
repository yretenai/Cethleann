using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Cethleann.Structure
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
    public struct TextDataTableHeader
    {
        public int Offset { get; set; }
        public int Size { get; set; }
    }
}
