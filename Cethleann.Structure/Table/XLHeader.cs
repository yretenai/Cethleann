namespace Cethleann.Structure.Table
{
    public struct XLHeader
    {
        public ushort Magic { get; set; }
        public ushort Version { get; set; }
        public ushort FileSize { get; set; }
        public ushort Types { get; set; }
        public ushort Sets { get; set; }
        public short Width { get; set; }
        public short TableOffset { get; set; }
        public short Unknown { get; set; }
    }
}
