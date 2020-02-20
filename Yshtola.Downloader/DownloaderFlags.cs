using Cethleann.Structure;
using DragonLib.CLI;

namespace Yshtola.Downloader
{
    public class DownloaderFlags : ICLIFlags
    {
        [CLIFlag("game-dir", Positional = 0, Help = "Game Directory", IsRequired = true, Category = "Downloader Options")]
        public string GameDir { get; set; }

        [CLIFlag("root-server", Positional = 1, Help = "Download root", IsRequired = true, Category = "Downloader Options")]
        public string Server { get; set; }

        [CLIFlag("out-dir", Positional = 2, Help = "Output directory", IsRequired = true, Category = "Downloader Options")]
        public string OutputDirectory { get; set; }

        [CLIFlag("dry", Aliases = new[] { "n" }, Help = "Just output download links", Category = "Downloader Options")]
        public bool Dry { get; set; }

        [CLIFlag("threads", Default = 1, Aliases = new[] { "t" }, Help = "Number of download threads", Category = "Downloader Options")]
        public int Threads { get; set; }

        [CLIFlag("game", Default = DataGame.None, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "Unbundler Options")]
        public DataGame GameId { get; set; }
    }
}
