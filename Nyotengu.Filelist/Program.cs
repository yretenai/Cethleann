using System.IO;
using DragonLib.CLI;
using DragonLib.IO;

namespace Nyotengu.Filelist
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Logger.PrintVersion("Nyotengu");
            var flags = CommandLineFlags.ParseFlags<FilelistFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null || string.IsNullOrWhiteSpace(flags.GameDir)) return;

            var nyotengu = new Cethleann.ManagedFS.Nyotengu(flags);
            nyotengu.LoadFileList(flags.FileList, flags.GameId);
            foreach (var rdb in Directory.GetFiles(flags.GameDir, "*.rdb")) nyotengu.AddDataFS(rdb);
            nyotengu.SaveGeneratedFileList(null, flags.GameId);
        }
    }
}
