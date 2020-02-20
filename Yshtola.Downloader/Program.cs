using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Cethleann.ManagedFS.Support;
using Cethleann.Structure;
using DragonLib;
using DragonLib.CLI;
using DragonLib.IO;

namespace Yshtola.Downloader
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Nyotengu");
            var flags = CommandLineFlags.ParseFlags<DownloaderFlags>(CommandLineFlags.PrintHelp, args);


            YshtolaSettings settings = flags.GameId switch
            {
                DataGame.DissidiaNT => new YshtolaDissidiaSettings(),
                DataGame.VenusVacation => new YshtolaVenusVacationSettings(),
                _ => default
            };
            if (settings == default)
            {
                Logger.Error("Yshtola", $"No decryption settings found for {flags.GameId}!");
                return;
            }

            using var fs = new Cethleann.ManagedFS.Yshtola(flags.GameId, settings)
            {
                Root = new[] { flags.GameDir, flags.OutputDirectory }
            };
            foreach (var tableName in settings.TableNames) fs.AddDataFS(tableName);
            var pending = new ConcurrentQueue<(string url, string destination, string size)>();
            var totalSize = 0UL;
            foreach (var table in fs.Tables)
            {
                for (var index = 0; index < table.Entries.Length; index++)
                {
                    var entry = table.Entries[index];
                    var pkgInfo = table.PackageInfo;
                    if (pkgInfo == null) continue;
                    var packageEntry = pkgInfo.InfoTable.Entries[index];
                    var filepath = entry.Path(table.Buffer, table.Header.Offset);
                    var path = Path.Combine(flags.OutputDirectory, filepath);
                    if (packageEntry.Version <= 0 || File.Exists(path) || File.Exists(Path.Combine(flags.GameDir, filepath))) continue;
                    pending.Enqueue(($"{flags.Server}/{packageEntry.Version}/{filepath}", path, ((ulong) entry.CompressedSize).GetHumanReadableBytes()));
                    var basedir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(basedir)) Directory.CreateDirectory(basedir);
                    totalSize += entry.CompressedSize;
                }
            }

            Logger.Warn("Yshtola", $"Going to download {totalSize} bytes ({totalSize.GetHumanReadableBytes()})");
            Parallel.ForEach(pending, new ParallelOptions
            {
                MaxDegreeOfParallelism = flags.Threads
            }, pair =>
            {
                var (url, dest, size) = pair;
                Logger.Info("Yshtola", url);
                if (flags.Dry) return;
                using var http = new WebClient();
                try
                {
                    http.DownloadFile(url, dest);
                    Logger.Info("Yshtola", $"Downloaded {size} to {dest}");
                }
                catch (Exception e)
                {
                    File.WriteAllBytes(dest, new byte[0]);
                    Logger.Error("Yshtola", e.Message);
                }
            });
        }
    }
}
