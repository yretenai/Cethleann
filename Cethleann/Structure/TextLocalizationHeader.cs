namespace Cethleann.Koei.Structure
{
    internal struct TextLocalizationHeader
    {
        public uint Magic { get; set; }
        public short FileSize { get; set; }
        public short LanguageCount { get; set; }
        public short Count { get; set; }
        public short Width { get; set; }
        public short Size { get; set; }
        public short Unknown1 { get; set; }
        public short Unknown2 { get; set; }
        public short Unkonwn3 { get; set; }
    }
}
