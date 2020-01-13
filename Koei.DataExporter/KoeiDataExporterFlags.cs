using System.Collections.Generic;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Koei.DataExporter
{
    [UsedImplicitly]
    public class KoeiDataExporterFlags : ICLIFlags
    {
        [CLIFlag("base-dir", Positional = 0, Help = "Base RomFS Directory", IsRequired = true, Category = "DataExporter Options")]
        public string BaseDirectory { get; set; }

        [CLIFlag("out-dir", Positional = 1, Help = "Extraction Directory", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; }

        [CLIFlag("patch-dir", Positional = 2, Help = "Patch RomFS Directory (needs an INFO0.bin file)", Category = "DataExporter Options")]
        public string PatchDirectory { get; set; }

        [UsedImplicitly]
        [CLIFlag("dlc-dirs", Positional = 3, Help = "List of DLC RomFS Directories", Category = "DataExporter Options")]
        public List<string> DLCDirectories { get; set; } = new List<string>();

        [CLIFlag("recursive", Aliases = new[] { "R" }, Help = "Recursively parse and unbundle files", Category = "DataExporter Options")]
        public bool Recursive { get; set; }
    }
}
