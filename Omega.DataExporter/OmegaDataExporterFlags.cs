using System.Collections.Generic;
using Cethleann.Unbundler;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Omega.DataExporter
{
    [UsedImplicitly]
    public class OmegaDataExporterFlags : UnbundlerFlags
    {
        [CLIFlag("base-dir", Positional = 0, Help = "Output Directories", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; }

        [CLIFlag("base-dir", Positional = 1, Help = "RomFS Directories", IsRequired = true, Category = "DataExporter Options")]
        [UsedImplicitly]
        public List<string> Directories { get; set; }
    }
}
