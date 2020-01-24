using System.Collections.Generic;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Cethleann.Unbundler
{
    public class CethleannUnbundlerFlags : UnbundlerFlags
    {
        [UsedImplicitly]
        [CLIFlag("paths", Positional = 0, IsRequired = true, Help = "List of Directories or Files", Category = "Unbundler Options")]
        public List<string> Paths { get; set; }
        
        [CLIFlag("bundle", Aliases = new []{ "b"}, Help = "Bundle directory as a file", Category = "Unbundler Options")]
        public bool Bundle { get; set; }
    }
}
