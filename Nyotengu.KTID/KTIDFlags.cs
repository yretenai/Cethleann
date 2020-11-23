using System.Collections.Generic;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Nyotengu.KTID
{
    [UsedImplicitly]
    public class KTIDFlags : ICLIFlags
    {
        [CLIFlag("obj-db-path", Positional = 0, IsRequired = true, Help = "Database file to load", Category = "KTID Options")]
        public string OBJDBPath { get; set; } = string.Empty;

        [CLIFlag("material-folder-path", Positional = 1, IsRequired = true, Help = "Material folder path", Category = "KTID Options")]
        public string MaterialFolderPath { get; set; } = string.Empty;

        [UsedImplicitly]
        [CLIFlag("paths", Positional = 2, IsRequired = true, Help = "KTID Paths to load", Category = "KTID Options")]
        public HashSet<string> Paths { get; set; } = new HashSet<string>();

        [CLIFlag("name", Help = "NAME database path", Category = "KTID Options")]
        public string? NDBPath { get; set; }

        [CLIFlag("filelist", Help = "File List to load. Unspecified is automatically determined based on GameId", Category = "KTID Options")]
        public string? FileList { get; set; }

        [CLIFlag("game", Default = "", Aliases = new[] { "g" }, Help = "Game being loaded", Category = "KTID Options")]
        public string GameId { get; set; } = "";
    }
}
