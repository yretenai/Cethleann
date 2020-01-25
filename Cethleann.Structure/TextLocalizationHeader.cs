namespace Cethleann.Structure
{
    public struct TextLocalizationHeader
    {
        public DataType Magic { get; set; }
        public ushort FileSize { get; set; }
        public ushort Unknown1 { get; set; }
        public ushort Sets { get; set; }
        public short Width { get; set; }
        public short TableOffset { get; set; }
        public short Unknown2 { get; set; }
    }
}
