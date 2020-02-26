using Cethleann.Structure;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Nyotengu.Database
{
    [UsedImplicitly]
    public class DatabaseFlags : ICLIFlags
    {
        [CLIFlag("path", Positional = 0, IsRequired = true, Help = "Database file to load", Category = "Database Options")]
        public string Path { get; set; } = string.Empty;

        [CLIFlag("namedb", Positional = 1, Help = "Name Database file to load", Category = "Database Options")]
        public string? NDBPath { get; set; }

        [CLIFlag("hash-all", Category = "Database Options")]
        public bool HashAll { get; set; }

        [CLIFlag("filelist", Help = "File List to load. Unspecified is automatically determined based on GameId", Category = "Database Options")]
        public string? FileList { get; set; }

        [CLIFlag("typeinfo-filter", Aliases = new[] { "f" }, Help = "TypeInfo filter", Category = "Database Options")]
        public string? TypeInfoFilter { get; set; }

        [CLIFlag("game", Default = DataGame.None, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "Database Options")]
        public DataGame GameId { get; set; }
    }
}
