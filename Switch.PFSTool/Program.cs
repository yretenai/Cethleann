using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Switch.PFSTool
{
    internal static class Program
    {
        private const int PFS0_MAGIC = 'P' << 0 | 'F' << 8 | 'S' << 16 | '0' << 24;

        private static readonly string[] ByteSizes = { "B", "KB", "MB", "GB", "TB" };

        private static string HumanFriendlySize(double size)
        {
            var order = 0;
            while (size >= 1024 && order < ByteSizes.Length - 1)
            {
                order++;
                size /= 1024.0d;
            }

            return $"{size:0.##} {ByteSizes[order]}";
        }

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Switch.PFSTool.exe in_file out_dir");
                return;
            }

            using var archive = File.OpenRead(args[0]);
            var buffer = new Span<byte>(new byte[0x10]);
            archive.Read(buffer);
            var header = MemoryMarshal.Read<PFS0Header>(buffer);
            if (header.Magic != PFS0_MAGIC)
            {
                Console.Error.WriteLine("Not a PFS0 file.");
                return;
            }

            buffer = new Span<byte>(new byte[0x18 * header.FileCount]);
            archive.Read(buffer);
            var entries = MemoryMarshal.Cast<byte, PFS0Entry>(buffer);
            var nameBlock = new Span<byte>(new byte[header.NameBlockSize]);
            archive.Read(nameBlock);
            var eob = archive.Position;

            var targetDir = args[1];
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            foreach (var entry in entries)
            {
                var filename = Encoding.UTF8.GetString(nameBlock.Slice(entry.StringOffset, nameBlock.IndexOf<byte>(0)).ToArray().TakeWhile(x => x != 0).ToArray());
                archive.Position = eob + entry.Offset;
                buffer = new Span<byte>(new byte[Math.Min(1024 * 1024 * 1024, entry.Size)]);
                Console.Write($"Dumping {filename} ({HumanFriendlySize(entry.Size)} in {HumanFriendlySize(buffer.Length)} blocks)... ");
                var read = 0L;
                var path = Path.Combine(targetDir, filename).Trim();
                using var file = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                while (read < entry.Size)
                {
                    var blockRead = archive.Read(buffer);
                    file.Write(buffer.Slice(0, blockRead));
                    read += blockRead;
                }

                Console.WriteLine($"{HumanFriendlySize(file.Length)} bytes written.");
            }
        }

        private struct PFS0Header
        {
            public int Magic { get; set; }
            public int FileCount { get; set; }
            public int NameBlockSize { get; set; }
            public int Reserved { get; set; }
        }

        private struct PFS0Entry
        {
            public long Offset { get; set; }
            public long Size { get; set; }
            public int StringOffset { get; set; }
            public int Unknown { get; set; }
        }
    }
}
