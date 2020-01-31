using Cethleann.Structure;
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

        [CLIFlag("game", Default = DataGame.DissidiaNT, Aliases = new[] { "g" }, ValidValues = new[] { nameof(DataGame.DissidiaNT) }, Help = "Game Type", Category = "DataExporter Options")]
        public DataGame Game { get; set; }

        [CLIFlag("manifest-only", Default = false, Help = "Only dump file manifest", Category = "DataExporter Options")]
        public bool ManifestOnly { get; set; }
    }
}
