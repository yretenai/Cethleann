using System.IO;
using Cethleann.ManagedFS;
using DragonLib.CLI;
using DragonLib.IO;

namespace Ninja.DataExporter
{
    static class Program
    {
        static void Main(string[] args)
        {
            var flags = CommandLineFlags.ParseFlags<NinjaDataExporterFlags>(CommandLineFlags.PrintHelp, args);

            var settings = flags.Game switch
            {
                InstallType.Dissidia => new DissidiaSettings(),
                _ => null
            };
            var yshtola = new Yshtola(flags.RootDirectory, settings);

            foreach (var entry in yshtola)
            {
                try
                {
                    var (data, filepath) = yshtola.ReadEntry(entry);
                    if (data.Length == 0)
                    {
                        Logger.Info("NINJA", $"{entry} is zero!");
                        continue;
                    }

                    while (filepath.StartsWith("\\") || filepath.StartsWith("/")) filepath = filepath.Substring(1);

                    var path = Path.Combine(flags.OutputDirectory, filepath);
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllBytes(path, data.ToArray());
                    Logger.Info("NINJA", path);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
