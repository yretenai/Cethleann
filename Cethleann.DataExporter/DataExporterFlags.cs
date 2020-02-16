using System.Collections.Generic;
using Cethleann.Unbundler;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Cethleann.DataExporter
{
    public class DataExporterFlags : UnbundlerFlags
    {
        [CLIFlag("out-dir", Positional = 0, Help = "Extraction Directory", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; }

        [UsedImplicitly]
        [CLIFlag("game-dirs", Positional = 1, Help = "List of Game Directories", IsRequired = true, Category = "DataExporter Options")]
        public List<string> GameDirs { get; set; } = new List<string>();

        [CLIFlag("filelist", Help = "File List to load. Unspecified is automatically determined based on GameId,", Category = "DataExporter Options")]
        public string FileList { get; set; }

        [CLIFlag("no-filelist", Help = "Don't use a filelist", Category = "DataExporter Options")]
        public bool NoFilelist { get; set; }

        [CLIFlag("flayn", Aliases = new[] { "linkdata" }, Help = "Game is contained in LINKDATA containers", Category = "ManagedFS Options")]
        public bool Flayn { get; set; }

        [CLIFlag("leonhart", Aliases = new[] { "linkarchive" }, Help = "Game is contained in LINKARCHIVE containers", Category = "ManagedFS Options")]
        public bool Leonhart { get; set; }

        [CLIFlag("mitsunari", Aliases = new[] { "linkbin" }, Help = "Game is contained in LNK containers", Category = "ManagedFS Options")]
        public bool Mitsunari { get; set; }

        [CLIFlag("nyotengu", Aliases = new[] { "rdb" }, Help = "Game is contained in RDB containers", Category = "ManagedFS Options")]
        public bool Nyotengu { get; set; }

        [CLIFlag("reisalin", Aliases = new[] { "pak" }, Help = "Game is contained in PAK containers", Category = "ManagedFS Options")]
        public bool Reisalin { get; set; }

        [CLIFlag("yshtola", Aliases = new[] { "pkg", "common" }, Help = "Game is contained in PKG containers (COMMON directory)", Category = "ManagedFS Options")]
        public bool Yshtola { get; set; }

        [CLIFlag("32bit", Aliases = new[] { "w" }, Help = "Parse older, 32-bit Atelier PAKs", Category = "Reisalin Options")]
        public bool A17 { get; set; }

        [CLIFlag("manifest-only", Help = "Only dump package manifest", Category = "Y'shtola Options")]
        public bool ManifestOnly { get; set; }
    }
}
