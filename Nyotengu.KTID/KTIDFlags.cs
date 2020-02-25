using System.Collections.Generic;
using Cethleann.Structure;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Nyotengu.KTID
{
    [UsedImplicitly]
    public class KTIDFlags : ICLIFlags
    {
        [CLIFlag("obj-db-path", Positional = 0, IsRequired = false, Help = "Database file to load", Category = "KTID Options")]
        public string OBJDBPath { get; set; }

        [CLIFlag("material-folder-path", Positional = 1, Help = "Material folder path", Category = "KTID Options")]
        public string MaterialFolderPath { get; set; }

        [UsedImplicitly]
        [CLIFlag("paths", Positional = 2, IsRequired = false, Help = "KTID Paths to load", Category = "KTID Options")]
        public HashSet<string> Paths { get; set; } = new HashSet<string>();

        [CLIFlag("name", Help = "NAME database path", Category = "KTID Options")]
        public string NDBPath { get; set; }

        [CLIFlag("filelist", Help = "File List to load. Unspecified is automatically determined based on GameId", Category = "KTID Options")]
        public string FileList { get; set; }

        [CLIFlag("game", Default = DataGame.None, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "KTID Options")]
        public DataGame GameId { get; set; }
    }
}
