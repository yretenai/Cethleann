using System.Collections.Generic;
using Cethleann.Structure;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Nyotengu.Database
{
    [UsedImplicitly]
    public class DatabaseFlags : ICLIFlags
    {
        [CLIFlag("path", Positional = 0, IsRequired = true, Help = "Database files to load", Category = "Database Options")]
        public HashSet<string> Paths { get; set; } = new HashSet<string>();

        [CLIFlag("ndb", Help = "Directory with namedb files when loading OBJDB files", Category = "Database Options")]
        public string? NDBPath { get; set; }

        [CLIFlag("hash", Help = "Hash NDB values", Category = "Database Options")]
        public bool HashNames { get; set; }

        [CLIFlag("hash", Help = "Hash NDB type values", Category = "Database Options")]
        public bool HashTypes { get; set; }

        [CLIFlag("hash-extra", Help = "Hash extra values", Category = "Database Options")]
        public bool HashExtra { get; set; }

        [CLIFlag("show-ktids", Help = "Show KTIDs rather than named value", Category = "Database Options")]
        public bool ShowKTIDs { get; set; }

        [CLIFlag("filelist", Help = "File List to load. Unspecified is automatically determined based on GameId", Category = "Database Options")]
        public string? FileList { get; set; }

        [CLIFlag("typeinfo-filter", Aliases = new[] { "f" }, Help = "TypeInfo filter", Category = "Database Options")]
        public string? TypeInfoFilter { get; set; }

        [CLIFlag("game", Default = DataGame.None, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "Database Options")]
        public DataGame GameId { get; set; }
    }
}
