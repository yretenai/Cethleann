using System;
using System.IO;
using Cethleann;
using Cethleann.ManagedFS;
using Cethleann.Structure;
using Cethleann.Unbundler;
using DragonLib.CLI;
using DragonLib.IO;

namespace Ninja.DataExporter
{
    static class Program
    {
        public static NinjaDataExporterFlags Flags { get; set; } = new NinjaDataExporterFlags();

        static void Main(string[] args)
        {
            Logger.PrintVersion("NINJA");
            Flags = CommandLineFlags.ParseFlags<NinjaDataExporterFlags>(CommandLineFlags.PrintHelp, args);

            if (Flags.GameId != DataGame.DissidiaNT)
            {
                ExportArchive();
                return;
            }

            ExportTable();
        }

        private static void ExportTable()
        {
            var settings = Flags.GameId switch
            {
                DataGame.DissidiaNT => new DissidiaSettings(),
                _ => null
            };
            if (settings == null) throw new NotImplementedException($"Game {Flags.GameId} is not supported!");

            var yshtola = new Yshtola(Flags.RootDirectory, settings);

            if (!Directory.Exists(Flags.OutputDirectory)) Directory.CreateDirectory(Flags.OutputDirectory);

            File.WriteAllBytes(Path.Combine(Flags.OutputDirectory, "manifest." + Flags.GameId.ToString("G").ToLower()), yshtola.Table.Table.ToArray());

            if (Flags.ManifestOnly) return;

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

                    var path = Path.Combine(Flags.OutputDirectory, filepath);
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

        private static void ExportArchive()
        {
            using var mitsunari = new Mitsunari(Flags.GameId);
            mitsunari.AddDataFS(Flags.RootDirectory);
            mitsunari.LoadFileList();
            ExtractAll(Flags.OutputDirectory, mitsunari);
        }

        private static void ExtractAll(string romfs, IManagedFS fs)
        {
            for (var index = 0; index < fs.EntryCount; index++)
            {
                var data = fs.ReadEntry(index);
                var dt = data.Span.GetDataType();
                var ext = UnbundlerLogic.GetExtension(data.Span);
                var pathBase = $@"{romfs}\{fs.GetFilename(index, ext, dt)}";
                UnbundlerLogic.TryExtractBlob(pathBase, data, false, Flags);
            }
        }
    }
}
