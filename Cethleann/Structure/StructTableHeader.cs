using System.Runtime.InteropServices;

namespace Cethleann.Structure
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0x40)]
    public struct StructTableHeader
    {
        public int Magic { get; set; }
        public int Count { get; set; }
        public int Size { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        public int Unknown5 { get; set; }
        public int Unknown6 { get; set; }
        public int Unknown7 { get; set; }
        public int Unknown8 { get; set; }
        public int Unknown9 { get; set; }
        public int UnknownA { get; set; }
        public int UnknownB { get; set; }
        public int UnknownC { get; set; }
        public int UnknownD { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
