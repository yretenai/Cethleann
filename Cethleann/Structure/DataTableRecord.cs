using System.Runtime.InteropServices;

namespace Cethleann.Structure
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
    internal struct DataTableRecord
    {
        public int Offset { get; set; }
        public int Size { get; set; }
    }
}
