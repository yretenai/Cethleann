using Cethleann.Unbundler;
using DragonLib.CLI;

namespace Softness.DataExporter
{
    public class SoftnessDataExporterFlags : UnbundlerFlags
    {
        [CLIFlag("output-dir", Positional = 0, Help = "Output Directory", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; }

        [CLIFlag("game-dir", Positional = 1, Help = "Game Directory", IsRequired = true, Category = "DataExporter Options")]
        public string GameDirectory { get; set; }

        [CLIFlag("no-filelist", Help = "Use Filelist", Category = "DataExporter Options")]
        public bool NoFilelist { get; set; }
    }
}
