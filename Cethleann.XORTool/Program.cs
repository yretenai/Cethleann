using System;
using System.Collections.Generic;
using System.IO;
using DragonLib.CLI;
using DragonLib.IO;

namespace Cethleann.XORTool
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Cethleann");
            var flags = CommandLineFlags.ParseFlags<XORFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;

            var files = new List<string>();
            foreach (var arg in flags.Paths)
            {
                if (!Directory.Exists(arg))
                {
                    files.Add(arg);
                    continue;
                }

                if (flags.Recursive) files.AddRange(Directory.GetFiles(arg, flags.Mask, SearchOption.TopDirectoryOnly));
            }

            foreach(var file in files) {
                Logger.Info("Cethleann", file);
                Span<byte> bytes = File.ReadAllBytes(file);
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] ^= (byte) flags.Xor;
                }
                File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(file)!, $"dec_{flags.Xor:X}_{Path.GetFileName(file)}"), bytes.ToArray());
            }
        }
    }
}
