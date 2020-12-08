using Cethleann.ManagedFS.Options;
using Cethleann.Unbundler;
using DragonLib.CLI;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace Cethleann.DataExporter
{
    public class DataExporterFlags : UnbundlerFlags, IReisalinOptions, INyotenguOptions, IFlaynOptions
    {
        [CLIFlag("out-dir", Positional = 0, Help = "Extraction Directory", IsRequired = true, Category = "DataExporter Options")]
        public string OutputDirectory { get; set; } = string.Empty;

        [UsedImplicitly]
        [CLIFlag("game-dirs", Positional = 1, Help = "List of Game Directories", IsRequired = true, Category = "DataExporter Options")]
        public List<string> GameDirs { get; set; } = new List<string>();

        [CLIFlag("filelist", Help = "File List to load. Unspecified is automatically determined based on GameId", Category = "DataExporter Options")]
        public string? FileList { get; set; }

        [CLIFlag("no-filelist", Help = "Don't use a filelist", Category = "DataExporter Options")]
        public bool NoFilelist { get; set; }

        [CLIFlag("flayn", Aliases = new[] { "linkdata" }, Help = "Game is contained in LINKDATA containers", Category = "ManagedFS Options")]
        public bool Flayn { get; set; }

        [CLIFlag("leonhart", Aliases = new[] { "linkarchive" }, Help = "Game is contained in LINKARCHIVE containers", Category = "ManagedFS Options")]
        public bool Leonhart { get; set; }

        [CLIFlag("mitsunari", Aliases = new[] { "linkbin", "lnk" }, Help = "Game is contained in LNK containers", Category = "ManagedFS Options")]
        public bool Mitsunari { get; set; }

        [CLIFlag("nyotengu", Aliases = new[] { "rdb" }, Help = "Game is contained in RDB containers", Category = "ManagedFS Options")]
        public bool Nyotengu { get; set; }

        [CLIFlag("zhao", Aliases = new[] { "prdb", "fdata" }, Help = "Game is contained in PRDB/FDATA containers", Category = "ManagedFS Options")]
        public bool Zhao { get; set; }

        [CLIFlag("reisalin", Aliases = new[] { "pak" }, Help = "Game is contained in PAK containers", Category = "ManagedFS Options")]
        public bool Reisalin { get; set; }

        [CLIFlag("yshtola", Aliases = new[] { "pkg", "common" }, Help = "Game is contained in PKG containers (COMMON directory)", Category = "ManagedFS Options")]
        public bool Yshtola { get; set; }

        [CLIFlag("yshtola-manifest-only", Help = "Only dump package manifest", Category = "Y'shtola Options")]
        public bool YshtolaManifestOnly { get; set; }

        [CLIFlag("nyotengu-generate-filelist", Help = "Generate CSV filelist", Hidden = true, Category = "Nyotengu Options")]
        public bool NyotenguGeneratedFileList { get; set; }

        #region IManagedFSOptions

        public bool ReisalinA17 { get; set; }
        public bool NyotenguPrefixFilenames { get; set; }
        public bool TinyFlayn { get; set; }

        #endregion
    }
}
