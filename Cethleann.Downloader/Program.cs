using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Cethleann.Structure.DataStructs;
using DragonLib;
using DragonLib.CLI;
using DragonLib.IO;

namespace Cethleann.Downloader
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Cethleann");
            var flags = CommandLineFlags.ParseFlags<DownloaderFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;

            Dictionary<string, FileListEntry> entries = JsonSerializer.Deserialize<Dictionary<string, FileListEntry>>(File.ReadAllText(flags.DataFile)) ?? throw new NullReferenceException();
            var pending = new ConcurrentQueue<(string url, string destination, string size)>();
            var totalSize = 0UL;
            foreach (var (name, entry) in entries)
            {
                if (!name.Equals(entry.FileName, StringComparison.InvariantCultureIgnoreCase)) Logger.Warn("Cethleann", $"{name} -> {entry.FileName}");
                var type = Path.GetExtension(entry.FileName).ToUpper().Substring(1);
                if (type == "DZ")
                {
                    var bname = Path.GetFileNameWithoutExtension(entry.FileName);
                    if (bname.Contains(".")) type = $"{Path.GetExtension(bname).ToUpper().Substring(1)}_{type}";
                }

                if (type == "SWF" && !flags.Swf) continue;
                var path = Path.Combine(flags.OutputDirectory, entry.Directory, type, entry.FileName);
                if (File.Exists(path)) continue;
                pending.Enqueue(($"{flags.Server}/{entry.Directory}/{entry.FileName}", path, entry.Size.GetHumanReadableBytes()));
                var basedir = Path.GetDirectoryName(path);
                if (!Directory.Exists(basedir)) Directory.CreateDirectory(basedir ?? "./");
                totalSize += entry.Size;
            }

            Logger.Warn("Cethleann", $"Going to download {totalSize} bytes ({totalSize.GetHumanReadableBytes()})");
            Parallel.ForEach(pending, new ParallelOptions
            {
                MaxDegreeOfParallelism = flags.Threads
            }, pair =>
            {
                var (url, dest, size) = pair;
                Logger.Info("Cethleann", url);
                if (flags.Dry) return;
                using var http = new WebClient();
                try
                {
                    http.DownloadFile(url, dest);
                    Logger.Info("Cethleann", $"Downloaded {size} to {dest}");
                }
                catch (Exception e)
                {
                    File.WriteAllBytes(dest, Array.Empty<byte>());
                    Logger.Error("Cethleann", e);
                }
            });
        }
    }
}
