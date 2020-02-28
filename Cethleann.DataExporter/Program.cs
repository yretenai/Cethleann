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
            Logger.PrintVersion("Cethleann");
            var flags = CommandLineFlags.ParseFlags<DataExporterFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;

            IManagedFS? fs = default;
            if (flags.Flayn)
            {
                fs = new Flayn(flags);
                ((Flayn) fs).LoadPatterns();
                foreach (var gamedir in flags.GameDirs) fs.AddDataFS(gamedir);
            }
            else if (flags.Leonhart)
            {
                fs = new Leonhart(flags);
                foreach (var gamedir in flags.GameDirs) fs.AddDataFS(gamedir);
            }
            else if (flags.Mitsunari)
            {
                fs = new Mitsunari(flags);
                foreach (var gamedir in flags.GameDirs) fs.AddDataFS(gamedir);
            }
            else if (flags.Nyotengu)
            {
                fs = new Nyotengu(flags);
                foreach (var rdb in flags.GameDirs.SelectMany(gamedir => Directory.GetFiles(gamedir, "*.rdb"))) fs.AddDataFS(rdb);

                ((Nyotengu) fs).LoadExtList();
            }
            else if (flags.Reisalin)
            {
                fs = new Reisalin(flags);
                foreach (var gamedir in flags.GameDirs.SelectMany(gameDir => Directory.GetFiles(gameDir, "*.pak"))) fs.AddDataFS(gamedir);
            }
            else if (flags.Yshtola)
            {
                YshtolaSettings? settings = flags.GameId switch
                {
                    DataGame.DissidiaNT => new YshtolaDissidiaSettings(),
                    DataGame.VenusVacation => new YshtolaVenusVacationSettings(),
                    _ => default
                };
                if (settings == default)
                {
                    Logger.Error("Cethleann", $"No decryption settings found for {flags.GameId}!");
                    return;
                }

                fs = new Yshtola(flags, settings);
                var yshtola = (Yshtola) fs;
                yshtola.Root = flags.GameDirs.ToArray();
                foreach (var tableName in settings.TableNames) fs.AddDataFS(tableName);
                if (!Directory.Exists(flags.OutputDirectory)) Directory.CreateDirectory(flags.OutputDirectory);
                for (var index = 0; index < yshtola.Tables.Count; index++)
                {
                    var table = yshtola.Tables[index];
                    var type = Path.GetDirectoryName(yshtola.Settings.TableNames[index]);
                    var name = $"manifest-{type ?? "COMMON"}.{flags.GameId.ToString("G").ToLower()}";
                    File.WriteAllBytes(Path.Combine(flags.OutputDirectory, name), table.Buffer.ToArray());
                }

                if (flags.YshtolaManifestOnly) return;
            }

            if (fs == null)
            {
                Logger.Error("Cethleann", "No FS specified! Prove --flayn, --reisalin, --leonhart, --mitsunari, --nyotengu, or --yshtola!");
                return;
            }

            if (!flags.NoFilelist) fs.LoadFileList(flags.FileList);
            if (flags.NyotenguGeneratedFileList && fs is Nyotengu nyotengu)
            {
                nyotengu.SaveGeneratedFileList(flags.FileList);
                return;
            }

            for (var index = 0; index < fs.EntryCount; index++)
            {
                try
                {
                    var data = fs.ReadEntry(index).Span;
                    var dt = data.GetDataType();
                    var ext = UnbundlerLogic.GetExtension(data);
                    var filepath = fs.GetFilename(index, ext, dt) ?? $"{index}.{ext}";
                    while (filepath.StartsWith("\\") || filepath.StartsWith("/")) filepath = filepath.Substring(1);
                    if (flags.Reisalin && filepath.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (data[4] == 0x78) data = StreamCompression.Decompress(data, -1, 1);
                        filepath = filepath.Substring(0, filepath.Length - 3);
                    }

                    var pathBase = $@"{flags.OutputDirectory}\{filepath}";
                    UnbundlerLogic.TryExtractBlob(pathBase, data, false, flags, false);
                }
                catch (Exception e)
                {
                    Logger.Error("Cethleann", e);
#if DEBUG
                    throw;
#endif
                }
            }

            fs.Dispose();
        }
    }
}
