using Cethleann.Unbundler;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Softness.Hash
{
    [PublicAPI]
    public class SoftnessHashFlags : UnbundlerFlags
    {
        [CLIFlag("rdb-dir", Positional = 0, Help = "Game Directory", IsRequired = true, Category = "Hasher Options")]
        public string GameDirectory { get; set; }

        [CLIFlag("type-id", Positional = 1, Help = "File Type Id", IsRequired = true, Category = "Hasher Options")]
        public string TypeId { get; set; }

        [CLIFlag("type-name", Positional = 2, Help = "Type Name", IsRequired = true, Category = "Hasher Options")]
        public string TypeName { get; set; }

        [CLIFlag("max", Default = 16, Help = "Max Filename Length", Category = "Hasher Options")]
        public int Max { get; set; }

        [CLIFlag("min", Default = 8, Help = "Min Filename Length", Category = "Hasher Options")]
        public int Min { get; set; }

        [CLIFlag("global-prefix", Default = "R_", Help = "Global Filename Prefix", Category = "Hasher Options")]
        public string GlobalPrefix { get; set; }

        [CLIFlag("prefix", Default = "", Help = "Filename Prefix", Category = "Hasher Options")]
        public string Prefix { get; set; }

        [CLIFlag("no-rdb", Default = false, Help = "Skip RDB Parsing", Category = "Hasher Options")]
        public bool NoRDB { get; set; }
    }
}
