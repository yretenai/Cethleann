using Cethleann.ManagedFS.Options;
using Cethleann.Structure;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Yshtola.Downloader
{
    [PublicAPI]
    public class DownloaderFlags : ICLIFlags, IManagedFSOptions
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

        public DataPlatform Platform { get; set; }
        public DataGame GameId { get; set; }
    }
}
