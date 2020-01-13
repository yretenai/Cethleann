using System.IO;
using Cethleann.ManagedFS;
using DragonLib.CLI;
using DragonLib.IO;

namespace Gust.DataExporter
{
    static class Program
    {
        static void Main(string[] args)
        {
            var flags = CommandLineFlags.ParseFlags<GustDataExporterFlags>(CommandLineFlags.PrintHelp, args);

            using var reisalin = new Reisalin();
            foreach (var location in flags.PAKLocations) reisalin.Mount(location, !flags.A17);

            foreach (var (entry, pak) in reisalin)
            {
                try
                {
                    var data = pak.ReadEntry(entry).ToArray();
                    if (data.Length == 0)
                    {
                        Logger.Error("GUST", $"{entry.Filename} is zero!");
                        continue;
                    }

                    var fn = entry.Filename;
                    while (fn.StartsWith("\\") || fn.StartsWith("/")) fn = fn.Substring(1);

                    var path = Path.Combine(flags.OutputDirectory, fn);
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllBytes(path, data);
                    Logger.Info("GUST", path);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
