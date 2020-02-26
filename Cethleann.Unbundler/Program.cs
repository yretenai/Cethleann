using System;
using System.Collections.Generic;
using System.IO;
using DragonLib.CLI;
using DragonLib.IO;

namespace Cethleann.Unbundler
{
    static class Program
    {
        static void Main(string[] args)
        {
            Logger.PrintVersion("Cethleann");
            var flags = CommandLineFlags.ParseFlags<UnbundlerToolFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;

            var files = new List<string>();
            foreach (var arg in flags.Paths)
            {
                if (!Directory.Exists(arg))
                {
                    files.Add(arg);
                    continue;
                }

                if (flags.Recursive) files.AddRange(Directory.GetFiles(arg, flags.Mask, SearchOption.AllDirectories));
            }

            foreach (var arg in files)
            {
                try
                {
                    Logger.Info("Cethleann", $"Extracting {Path.GetFileName(arg)}...");
                    var data = File.ReadAllBytes(arg);
                    var pathBase = arg;
                    if (!arg.EndsWith(".text"))
                    {
                        if (Path.GetFileName(arg) == Path.GetFileNameWithoutExtension(arg))
                            pathBase += "_contents";
                        else
                            pathBase = Path.Combine(Path.GetDirectoryName(arg) ?? string.Empty, Path.GetFileNameWithoutExtension(arg));
                    }

                    UnbundlerLogic.TryExtractBlob(pathBase, data, true, flags, true);
                }
                catch (Exception e)
                {
                    Logger.Error("Cethleann", e);
#if DEBUG
                    throw;
#endif
                }
            }
        }
    }
}
