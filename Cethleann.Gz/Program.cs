using System;
using System.Collections.Generic;
using System.IO;
using Cethleann.Compression;
using DragonLib.CLI;
using DragonLib.IO;

namespace Cethleann.Gz
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Cethleann");
            var flags = CommandLineFlags.ParseFlags<GzFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;

            var files = new List<string>();
            foreach (var arg in flags.Paths)
            {
                if (!Directory.Exists(arg))
                {
                    files.Add(arg);
                    continue;
                }

                files.AddRange(Directory.GetFiles(arg, flags.Mask, SearchOption.AllDirectories));
            }

            Func<byte[], byte[]>? method = null;
            string? ext = null;
            if (flags.IsDz)
            {
                ext = ".dz";
                if (flags.Compress) method = bytes => DzCompression.Compress(bytes, flags.BlockSize, flags.Alignment).ToArray();
                else method = bytes => DzCompression.Decompress(bytes, flags.Alignment).ToArray();
            }
            else if (flags.IsStream)
            {
                ext = ".zl";
                if (flags.Compress) method = bytes => StreamCompression.Compress(bytes, flags.BlockSize).ToArray();
                else method = bytes => StreamCompression.Decompress(bytes, flags.Length, flags.Type, flags.BlockSize, flags.PrefixedSize).ToArray();
            }
            else if (flags.IsStream8000)
            {
                ext = ".z";
                if (flags.Compress) method = bytes => Stream8000Compression.Compress(bytes, flags.BlockSize).ToArray();
                else method = bytes => Stream8000Compression.Decompress(bytes, flags.Length).ToArray();
            }
            else if (flags.IsTable)
            {
                ext = ".gz";
                if (flags.Compress) method = bytes => TableCompression.Compress(bytes, flags.BlockSize).ToArray();
                else method = bytes => TableCompression.Decompress(bytes).ToArray();
            }

            if (method == null)
            {
                Logger.Error("Cethleann", "You must specify a compression method!");
                return;
            }

            foreach (var file in files)
            {
                var target = flags.Compress ? file + ext : Path.Combine(Path.GetDirectoryName(file) ?? "dir", Path.GetFileNameWithoutExtension(file) ?? "file");
                var baseTarget = target;
                var modulo = 0;
                while (File.Exists(target) && flags.Compress) target = baseTarget + $"_{++modulo}";
                Logger.Info("Cethleann", $"{(flags.Compress ? "C" : "Dec")}ompressing {Path.GetFileName(file)} to {target}");

                File.WriteAllBytes(target, method(File.ReadAllBytes(file)));

                if (flags.Delete) File.Delete(file);
            }
        }
    }
}
