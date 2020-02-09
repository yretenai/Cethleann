using System.Collections.Generic;
using Cethleann.Unbundler;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Koei.DataExporter
{
    [UsedImplicitly]
    public class KoeiDataExporterFlags : UnbundlerFlags
    {
        [CLIFlag("idx", Default = "DATA0", Help = "IDX Filename (without extension)", Category = "DataExporter Options")]
        public string IDXHint { get; set; }

        [CLIFlag("bin", Default = "DATA1", Help = "BIN Filename (without extension)", Category = "DataExporter Options")]
        public string BINHint { get; set; }

        [CLIFlag("link-prefix", Help = "Use LINKDATA filename as unknown path prefix", Category = "DataExporter Options")]
        public bool UseLinkdataPrefix { get; set; }

        [CLIFlag("filelist", Help = "File List filename to load, relative to the exe (unspecified is automatically determined based on GameId)", Category = "DataExporter Options")]
        public string FileList { get; set; }

        [CLIFlag("base-dir", Positional = 0, Help = "Base RomFS Directory", IsRequired = true, Category = "DataExporter Options")]
        public string BaseDirectory { get; set; }

        [CLIFlag("out-dir", Positional = 1, Help = "Extraction Directory", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; }

        [CLIFlag("patch-dir", Positional = 2, Help = "Patch RomFS Directory (needs an INFO0.bin file)", Category = "DataExporter Options")]
        public string PatchDirectory { get; set; }

        [UsedImplicitly]
        [CLIFlag("dlc-dirs", Positional = 3, Help = "List of DLC RomFS Directories", Category = "DataExporter Options")]
        public List<string> DLCDirectories { get; set; }
    }
}
