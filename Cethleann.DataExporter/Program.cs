using System;
using System.IO;
using System.Linq;
using Cethleann.Compression;
using Cethleann.ManagedFS;
using Cethleann.ManagedFS.Support;
using Cethleann.Structure;
using Cethleann.Unbundler;
using DragonLib.CLI;
using DragonLib.IO;

namespace Cethleann.DataExporter
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("CETH");
            var flags = CommandLineFlags.ParseFlags<DataExporterFlags>(CommandLineFlags.PrintHelp, args);

            IManagedFS fs = default;
            if (flags.Flayn)
            {
                fs = new Flayn(flags.GameId);
                foreach (var gamedir in flags.GameDirs) fs.AddDataFS(gamedir);
            }
            else if (flags.Leonhart)
            {
                fs = new Leonhart(flags.GameId);
                foreach (var gamedir in flags.GameDirs) fs.AddDataFS(gamedir);
            }
            else if (flags.Mitsunari)
            {
                fs = new Mitsunari(flags.GameId);
                foreach (var gamedir in flags.GameDirs) fs.AddDataFS(gamedir);
            }
            else if (flags.Nyotengu)
            {
                fs = new Nyotengu(flags.GameId);
                foreach (var rdb in flags.GameDirs.SelectMany(gamedir => Directory.GetFiles(gamedir, "*.rdb"))) fs.AddDataFS(rdb);

                ((Nyotengu) fs).LoadExtList();
            }
            else if (flags.Reisalin)
            {
                fs = new Reisalin(flags.GameId);
                foreach (var gamedir in flags.GameDirs) fs.AddDataFS(gamedir);
            }
            else if (flags.Yshtola)
            {
                YshtolaSettings settings = flags.GameId switch
                {
                    DataGame.DissidiaNT => new YshtolaDissidiaSettings(),
                    DataGame.VenusVacation => new YshtolaVenusVacationSettings(),
                    _ => default
                };
                if (settings == default)
                {
                    Logger.Error("CETH", $"No decryption settings found for {flags.GameId}!");
                    return;
                }

                fs = new Yshtola(flags.GameId, flags.GameDirs[0], settings);
                fs.AddDataFS(flags.GameDirs[0]);
                var yshtola = (Yshtola) fs;
                for (var index = 0; index < yshtola.Tables.Count; index++)
                {
                    var table = yshtola.Tables[index];
                    var type = Path.GetDirectoryName(yshtola.Settings.TableNames[index]);
                    var name = $"manifest-{type ?? "COMMON"}.{flags.GameId.ToString("G").ToLower()}";
                    File.WriteAllBytes(Path.Combine(flags.OutputDirectory, name), table.Buffer.ToArray());
                }

                if (flags.ManifestOnly) return;
            }

            if (fs == default)
            {
                Logger.Error("CETH", "No FS specified! Prove --flayn, --reisalin, --leonhart, --mitsunari, --nyotengu, or --yshtola!");
                return;
            }

            if (!flags.NoFilelist) fs.LoadFileList(flags.FileList);

            for (var index = 0; index < fs.EntryCount; index++)
            {
                var data = fs.ReadEntry(index).Span;
                var dt = data.GetDataType();
                var ext = UnbundlerLogic.GetExtension(data);
                var filepath = fs.GetFilename(index, ext, dt);
                while (filepath.StartsWith("\\") || filepath.StartsWith("/")) filepath = filepath.Substring(1);
                if (flags.Reisalin && filepath.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (data[4] == 0x78) data = StreamCompression.Decompress(data, -1, 1);
                    filepath = filepath.Substring(0, filepath.Length - 3);
                }

                var pathBase = $@"{flags.OutputDirectory}\{filepath}";
                UnbundlerLogic.TryExtractBlob(pathBase, data, false, flags);
            }

            fs.Dispose();
        }
    }
}
