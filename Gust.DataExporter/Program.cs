using System;
using System.IO;
using Cethleann.Koei;
using Cethleann.ManagedFS;
using Cethleann.Unbundler;
using DragonLib.CLI;
using DragonLib.IO;

namespace Gust.DataExporter
{
    static class Program
    {
        private static GustDataExporterFlags Flags;

        static void Main(string[] args)
        {
            Logger.PrintVersion("GUST");
            Flags = CommandLineFlags.ParseFlags<GustDataExporterFlags>(CommandLineFlags.PrintHelp, args);

            using var reisalin = new Reisalin(Flags.GameId);
            foreach (var location in Directory.GetFiles(Flags.GameDir, "*.PAK")) reisalin.AddDataFS(location, !Flags.A17);

            ExtractAll(Flags.OutputDirectory, reisalin);
        }


        private static void ExtractAll(string romfs, IManagedFS fs)
        {
            for (var index = 0; index < fs.EntryCount; index++)
            {
                var data = fs.ReadEntry(index);
                var filepath = fs.GetFilename(index);
                while (filepath.StartsWith("\\") || filepath.StartsWith("/")) filepath = filepath.Substring(1);
                if (filepath.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase) && data.Span[4] == 0x78)
                {
                    data = Compression.Decompress(data.Span, -1, 1).ToArray();
                    filepath = filepath.Substring(0, filepath.Length - 3);
                }

                var pathBase = $@"{romfs}\{filepath}";
                UnbundlerLogic.TryExtractBlob(pathBase, data, false, Flags);
            }
        }
    }
}
