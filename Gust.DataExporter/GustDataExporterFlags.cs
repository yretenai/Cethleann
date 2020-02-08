using Cethleann.Unbundler;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Gust.DataExporter
{
    [UsedImplicitly]
    public class GustDataExporterFlags : UnbundlerFlags
    {
        [CLIFlag("out-dir", Positional = 0, Help = "Extraction Directory", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; }

        [UsedImplicitly]
        [CLIFlag("game-dir", Positional = 1, Help = "Game Directory", IsRequired = true, Category = "DataExporter Options")]
        public string GameDir { get; set; }

        [CLIFlag("a17", Aliases = new[] { "old" }, Help = "Parse older, 32-bit PAKs", Category = "DataExporter Options")]
        public bool A17 { get; set; }
    }
}
