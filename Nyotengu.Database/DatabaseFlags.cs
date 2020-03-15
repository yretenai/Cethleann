using System.Collections.Generic;
using Cethleann.Structure;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Nyotengu.Database
{
    [UsedImplicitly]
    public class DatabaseFlags : ICLIFlags
    {
        [UsedImplicitly]
        [CLIFlag("path", Positional = 0, IsRequired = true, Help = "Database files to load", Category = "Database Options")]
        public HashSet<string> Paths { get; set; } = new HashSet<string>();

        [UsedImplicitly]
        [CLIFlag("ndb", Help = "Directories with namedb files, or NDB file when loading OBJDB files", Category = "Database Options")]
        public HashSet<string>? NDBPaths { get; set; } = new HashSet<string>();

        [CLIFlag("hash-types", Aliases = new[] { "T" }, Help = "Hash NDB type values", Category = "Database Options")]
        public bool HashTypes { get; set; }

        [CLIFlag("hash-extra", Aliases = new[] { "E" }, Help = "Hash extra values", Category = "Database Options")]
        public bool HashExtra { get; set; }

        [CLIFlag("create-filelist", Aliases = new[] { "F" }, Help = "Create fielelist with this namespace", Category = "Database Options")]
        public bool CreateFilelist { get; set; }

        [CLIFlag("show-ktids", Help = "Show KTIDs rather than named value", Category = "Database Options")]
        public bool ShowKTIDs { get; set; }

        [CLIFlag("create-missinglist", Aliases = new[] { "missing" }, Help = "Dump missing property hashes to filelist-MissingProperties-rdb.csv", Category = "Database Options", Hidden = true)]
        public bool CreateMissingList { get; set; }

        [CLIFlag("filelist", Help = "File List to load. Unspecified is automatically determined based on GameId", Category = "Database Options")]
        public string? FileList { get; set; }

        [CLIFlag("typeinfo-filter", Aliases = new[] { "f" }, Help = "TypeInfo filter", Category = "Database Options")]
        public string? TypeInfoFilter { get; set; }

        [CLIFlag("game", Default = DataGame.None, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "Database Options")]
        public DataGame GameId { get; set; }
    }
}
