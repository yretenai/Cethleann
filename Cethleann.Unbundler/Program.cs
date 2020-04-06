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

            foreach (var file in files)
            {
                try
                {
                    Logger.Info("Cethleann", $"Extracting {Path.GetFileName(file)}...");
                    var data = File.ReadAllBytes(file);
                    var pathBase = file;
                    if (!file.EndsWith(".text"))
                    {
                        if (Path.GetFileName(file) == Path.GetFileNameWithoutExtension(file))
                            pathBase += "_contents";
                        else
                            pathBase = Path.Combine(Path.GetDirectoryName(file) ?? string.Empty, Path.GetFileNameWithoutExtension(file));
                    }

                    if (UnbundlerLogic.TryExtractBlob(pathBase, data, true, flags, true) == 0) continue;
                    
                    if(flags.Delete) File.Delete(file);
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
