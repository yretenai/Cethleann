using System.Collections.Generic;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Cethleann.XORTool
{
    [PublicAPI]
    public class XORFlags : ICLIFlags
    {
        [CLIFlag("recursive", Default = true, Aliases = new[] { "R" }, Help = "Recursively parse and unbundle files", Category = "XOR Options")]
        public bool Recursive { get; set; }

        [CLIFlag("mask", Default = "N1*", Help = "Filename mask for recursive search", Category = "XOR Options")]
        public string Mask { get; set; } = "N1*";

        [CLIFlag("paths", Positional = 0, Help = "Encrypted Datas Files", IsRequired = true, Category = "XOR Options")]
        public List<string> Paths { get; set; } = new List<string>();

        [CLIFlag("xor", Default = 0xFF, Aliases = new[] { "x" }, Help = "XOR Constant", Category = "XOR Options")]
        public int Xor { get; set; }
    }
}
