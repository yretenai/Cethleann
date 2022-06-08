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

        [CLIFlag("linkdata", Help = "Game is contained in LINKDATA containers", Category = "ManagedFS Options")]
        public bool LINKDATA { get; set; }

        [CLIFlag("linkarchive", Help = "Game is contained in LINKARCHIVE containers", Category = "ManagedFS Options")]
        public bool LINKARCHIVE { get; set; }

        [CLIFlag("lnk", Aliases = new[] { "linkbin" }, Help = "Game is contained in LNK containers", Category = "ManagedFS Options")]
        public bool LNK { get; set; }

        [CLIFlag("rdb", Help = "Game is contained in RDB containers", Category = "ManagedFS Options")]
        public bool RDB { get; set; }

        [CLIFlag("prdb", Help = "Game is contained in PRDB containers that end with .hash", Category = "ManagedFS Options")]
        public bool PRDB { get; set; }

        [CLIFlag("rdx", Help = "Game is contained in RDX containers", Category = "ManagedFS Options")]
        public bool RDX { get; set; }

        [CLIFlag("pak", Help = "Game is contained in PAK containers", Category = "ManagedFS Options")]
        public bool PAK { get; set; }

        [CLIFlag("pkg", Help = "Game is contained in PKG containers (COMMON directory)", Category = "ManagedFS Options")]
        public bool PKG { get; set; }

        [CLIFlag("pkg-manifest-only", Help = "Only dump package manifest", Category = "Y'shtola Options")]
        public bool PKGManifestOnly { get; set; }

        [CLIFlag("rdb-generate-filelist", Help = "Generate CSV filelist", Hidden = true, Category = "Nyotengu Options")]
        public bool RDBGeneratedFileList { get; set; }

        #region IManagedFSOptions

        public bool PAKA17 { get; set; }

        public bool PAKKeyFix { get; set; }
        public bool RDBPrefixFilenames { get; set; }
        public bool TinyLINKDATA { get; set; }

        #endregion
    }
}
