using Cethleann.Unbundler;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Ninja.DataExporter
{
    [UsedImplicitly]
    public class NinjaDataExporterFlags : UnbundlerFlags
    {
        [CLIFlag("root-dir", Positional = 0, Help = "Installation directory of the game", IsRequired = true, Category = "DataExporter Options")]
        public string RootDirectory { get; set; }

        [CLIFlag("out-dir", Positional = 1, Help = "Extraction Directory", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; }

        [CLIFlag("manifest-only", Help = "Only dump file manifest", Category = "DataExporter Options")]
        public bool ManifestOnly { get; set; }
    }
}
