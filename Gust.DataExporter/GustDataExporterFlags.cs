using System.Collections.Generic;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Gust.DataExporter
{
    [UsedImplicitly]
    public class GustDataExporterFlags : ICLIFlags
    {
        [CLIFlag("out-dir", Positional = 0, Help = "Extraction Directory", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; }

        [UsedImplicitly]
        [CLIFlag("pak-dirs", Positional = 1, Help = "List of PAK Directories/Files", IsRequired = true, Category = "DataExporter Options")]
        public List<string> PAKLocations { get; set; }

        [CLIFlag("a17", Aliases = new[] { "old" }, Help = "Parse older, 32-bit PAKs", Category = "DataExporter Options")]
        public bool A17 { get; set; }
    }
}
