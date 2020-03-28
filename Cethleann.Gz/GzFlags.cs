using System.Collections.Generic;
using Cethleann.Structure;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Cethleann.Gz
{
    [UsedImplicitly]
    [PublicAPI]
    public class GzFlags : ICLIFlags
    {
        [CLIFlag("d", Aliases = new[] { "dz", "deflate" }, Help = "Zlib Deflate Compression (used in DFFOO)", Category = "Gz Options")]
        public bool IsDz { get; set; }

        [CLIFlag("8", Aliases = new[] { "stream8k", "8000" }, Help = "Stream8000 Compression (used in DFFNT) NEEDS --length to be set!", Category = "Gz Options")]
        public bool IsStream8000 { get; set; }

        [CLIFlag("s", Aliases = new[] { "stream" }, Help = "Stream Compression (used in many KTGL games)", Category = "Gz Options")]
        public bool IsStream { get; set; }

        [CLIFlag("g", Aliases = new[] { "gz", "table" }, Help = "Zlib Table Compression (used in FETH)", Category = "Gz Options")]
        public bool IsTable { get; set; }

        [CLIFlag("l", Aliases = new[] { "length" }, Help = "Length for Stream8000 decompression", Category = "Gz Options")]
        public int Length { get; set; }

        [CLIFlag("p", Default = false, Aliases = new[] { "prefixed-size" }, Help = "Decompressed Size is prefixed to the stream", Category = "Gz Options")]
        public bool PrefixedSize { get; set; }

        [CLIFlag("c", Aliases = new[] { "compress" }, Help = "Compress files rather than decompress", Category = "Gz Options")]
        public bool Compress { get; set; }

        [CLIFlag("b", Default = 0x4000, Aliases = new[] { "block" }, Help = "(de)compress in Blocks this size", Category = "Gz Options")]
        public int BlockSize { get; set; } = 0x4000;

        [CLIFlag("a", Default = 0x80, Aliases = new[] { "alignment" }, Help = "Block Alignment, used for Dz", Category = "Gz Options")]
        public int Alignment { get; set; } = 0x80;

        [CLIFlag("t", Default = DataCompression.Deflate, Aliases = new[] { "type" }, Help = "Stream compression type, used for Stream", Category = "Gz Options")]
        public DataCompression Type { get; set; } = DataCompression.Deflate;

        [UsedImplicitly]
        [CLIFlag("paths", Positional = 0, Help = "Files or directories to (de)compress", Category = "Gz Options")]
        public List<string> Paths { get; set; } = new List<string>();

        [CLIFlag("mask", Default = "*", Help = "Filename mask for recursive search", Category = "Gz Options")]
        public string Mask { get; set; } = "*";
    }
}
