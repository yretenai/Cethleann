using System.IO;
using Cethleann.ManagedFS;
using Cethleann.Structure;
using DragonLib.CLI;
using DragonLib.IO;

namespace Ninja.DataExporter
{
    static class Program
    {
        static void Main(string[] args)
        {
            Logger.PrintVersion("NINJA");
            var flags = CommandLineFlags.ParseFlags<NinjaDataExporterFlags>(CommandLineFlags.PrintHelp, args);

            var settings = flags.Game switch
            {
                DataGame.DissidiaNT => new DissidiaSettings(),
                _ => null
            };
            var yshtola = new Yshtola(flags.RootDirectory, settings);

            if (!Directory.Exists(flags.OutputDirectory)) Directory.CreateDirectory(flags.OutputDirectory);

            File.WriteAllBytes(Path.Combine(flags.OutputDirectory, "manifest." + flags.Game.ToString("G").ToLower()), yshtola.Table.Table.ToArray());

            if (flags.ManifestOnly) return;

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
