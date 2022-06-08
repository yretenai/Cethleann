using Cethleann.ManagedFS.Options;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Nyotengu.Filelist
{
    [UsedImplicitly]
    public class FilelistFlags : ICLIFlags, INyotenguOptions
    {
        [CLIFlag("game-dir", Positional = 0, Help = "Game Directorie", IsRequired = true, Category = "Filelist Options")]
        public string? GameDir { get; set; }

        [CLIFlag("filelist", Help = "File List to load. Unspecified is automatically determined based on GameId", Category = "Database Options")]
        public string? FileList { get; set; }

        [CLIFlag("game", Default = "", Aliases = new[] { "g" }, Help = "Game being loaded", Category = "Database Options")]
        public string GameId { get; set; } = "";

        public bool RDBPrefixFilenames { get; set; }
    }
}
