using Cethleann.Structure;

namespace Cethleann.Compression
{
    public class CompressionOptions
    {
        public int             BlockSize        { get; set; } = 0x10000;
        public int             Alignment        { get; set; } = 0x80;
        public int             CompressionLevel { get; set; } = 9;
        public long            Length           { get; set; } = 0x80;
        public bool            ForceLastBlock   { get; set; }
        public bool            PrefixSize       { get; set; }
        public DataCompression Type             { get; set; } = DataCompression.Deflate;
        public bool            Verify           { get; set; } = false;

        public static readonly CompressionOptions Default = new CompressionOptions();
    }
}
