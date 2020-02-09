using System.Collections.Generic;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Softness.Hasher
{
    [PublicAPI]
    public class SoftnessHasherFlags : ICLIFlags
    {
        [CLIFlag("prefix", Default = "R_", Help = "Hash Prefix", Category = "Hasher Options")]
        public string Prefix { get; set; }

        [CLIFlag("format", Help = "File type of the filename", Category = "Hasher Options")]
        public string Format { get; set; }

        [CLIFlag("raw", Default = false, Help = "Skip string formatting", Category = "Hasher Options")]
        public bool Raw { get; set; }

        [UsedImplicitly]
        [CLIFlag("strings", Positional = 0, Hidden = true)]
        public HashSet<string> Strings { get; set; } = new HashSet<string>();
    }
}
