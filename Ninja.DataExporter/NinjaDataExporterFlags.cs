using DragonLib.CLI;
using JetBrains.Annotations;

namespace Ninja.DataExporter
{
    [UsedImplicitly]
    public class NinjaDataExporterFlags : ICLIFlags
    {
        [CLIFlag("root-dir", Positional = 0, Help = "Installation directory of the game", IsRequired = true, Category = "DataExporter Options")]
        public string RootDirectory { get; set; }

        [CLIFlag("out-dir", Positional = 1, Help = "Extraction Directory", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; }

        [CLIFlag("game", Default = InstallType.Dissidia, Aliases = new[] { "g" }, Help = "Game Type", Category = "DataExporter Options")]
        public InstallType Game { get; set; }
    }
}
