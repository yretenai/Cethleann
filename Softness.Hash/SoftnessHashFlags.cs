using Cethleann.Unbundler;
using DragonLib.CLI;

namespace Softness.DataExporter
{
    public class SoftnessHashFlags : UnbundlerFlags
    {
        [CLIFlag("rdb-dir", Positional = 0, Help = "Game Directory", IsRequired = true, Category = "Hasher Options")]
        public string GameDirectory { get; set; }

        [CLIFlag("type-id", Positional = 1, Help = "File Type Id", IsRequired = true, Category = "Hasher Options")]
        public string TypeId { get; set; }

        [CLIFlag("type-name", Positional = 2, Help = "Type Name", IsRequired = true, Category = "Hasher Options")]
        public string TypeName { get; set; }

        [CLIFlag("max", Default = 32, Help = "Max Filename Length", Category = "Hasher Options")]
        public int Max { get; set; }

        [CLIFlag("min", Default = 4, Help = "Min Filename Length", Category = "Hasher Options")]
        public int Min { get; set; }

        [CLIFlag("prefix", Default = "R_", Help = "Filename Prefix", Category = "Hasher Options")]
        public string Prefix { get; set; }
    }
}
