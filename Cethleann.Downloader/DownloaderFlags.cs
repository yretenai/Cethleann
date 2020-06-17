using DragonLib.CLI;
using JetBrains.Annotations;

namespace Cethleann.Downloader
{
    [PublicAPI]
    public class DownloaderFlags : ICLIFlags
    {
        [CLIFlag("data-file", Positional = 0, Help = "File List Data File", IsRequired = true, Category = "Downloader Options")]
        public string DataFile { get; set; } = string.Empty;

        [CLIFlag("root-server", Positional = 1, Help = "Download root", IsRequired = true, Category = "Downloader Options")]
        public string Server { get; set; } = string.Empty;

        [CLIFlag("out-dir", Positional = 2, Help = "Output directory", IsRequired = true, Category = "Downloader Options")]
        public string OutputDirectory { get; set; } = string.Empty;

        [CLIFlag("dry", Aliases = new[] { "n" }, Help = "Just output download links", Category = "Downloader Options")]
        public bool Dry { get; set; }

        [CLIFlag("threads", Default = 1, Aliases = new[] { "t" }, Help = "Number of download threads", Category = "Downloader Options")]
        public int Threads { get; set; }

        [CLIFlag("swf", Default = false, Aliases = new[] { "s" }, Help = "Download SWF files", Category = "Downloader Options")]
        public bool Swf { get; set; }
    }
}
