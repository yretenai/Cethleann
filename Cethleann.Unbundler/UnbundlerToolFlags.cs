using System.Collections.Generic;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Cethleann.Unbundler
{
    public class UnbundlerToolFlags : UnbundlerFlags
    {
        [UsedImplicitly]
        [CLIFlag("paths", Positional = 0, IsRequired = true, Help = "List of Directories or Files", Category = "Unbundler Options")]
        public List<string> Paths { get; set; } = new List<string>();

        [CLIFlag("mask", Default = "*", Help = "Filename mask for recursive search", Category = "Unbundler Options")]
        public string Mask { get; set; } = "*";
    }
}
