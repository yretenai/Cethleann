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

            var options = new CompressionOptions
            {
                BlockSize        = flags.BlockSize,
                Alignment        = flags.Alignment,
                CompressionLevel = flags.Level,
                Length           = flags.Length,
                ForceLastBlock   = flags.CompressLast,
                PrefixSize       = flags.PrefixedSize,
                Type             = flags.Type,
            };
            if (flags.IsDz)
            {
                ext = ".dz";
                if (flags.Compress) method = bytes => DzCompression.Compress(bytes, options).ToArray();
                else method                = bytes => DzCompression.Decompress(bytes, options).ToArray();
            }
            else if (flags.IsStream)
            {
                ext = ".zl";
                if (flags.Compress) method = bytes => StreamCompression.Compress(bytes, options).ToArray();
                else method                = bytes => StreamCompression.Decompress(bytes, options).ToArray();
            }
            else if (flags.IsStream8000)
            {
                ext = ".z";
                if (flags.Compress) method = bytes => Stream8000Compression.Compress(bytes, options).ToArray();
                else method                = bytes => Stream8000Compression.Decompress(bytes, options).ToArray();
            }
            else if (flags.IsTable)
            {
                ext = ".gz";
                if (flags.Compress) method = bytes => TableCompression.Compress(bytes, options).ToArray();
                else method                = bytes => TableCompression.Decompress(bytes, options).ToArray();
            }

            if (method == null)
            {
                Logger.Error("Cethleann", "You must specify a compression method!");
                return;
            }

            foreach (var file in files)
            {
                var target = flags.Compress ? file + ext : Path.Combine(Path.GetDirectoryName(file) ?? "", Path.GetFileNameWithoutExtension(file));
                var baseTarget = target;
                var modulo = 0;
                while (File.Exists(target) && flags.Compress) target = baseTarget + $"_{++modulo}";
                Logger.Info("Cethleann", $"{(flags.Compress ? "C" : "Dec")}ompressing {Path.GetFileName(file)} to {target}");

                var bytes = File.ReadAllBytes(file);
                if (bytes.Length == 0)
                {
                    Logger.Info("Cethleann", $"{Path.GetFileName(file)} is empty");
                    continue;
                }

                File.WriteAllBytes(target, method(bytes));

                if (flags.Delete) File.Delete(file);
            }
        }
    }
}
