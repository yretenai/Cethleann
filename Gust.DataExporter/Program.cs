using System;
using System.IO;
using System.Linq;
using Cethleann.ManagedFS;
using DragonLib.IO;

namespace Gust.DataExporter
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Logger.Error("GUST", "Usage: Gust.DataExporter.exe output PAK_directory_or_file...");
                return;
            }

            using var reisalin = new Reisalin();
            var output = args[0];
            foreach (var arg in args.Skip(1)) reisalin.Mount(arg);

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

                    var path = Path.Combine(output, fn);
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
