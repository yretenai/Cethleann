using Cethleann;
using Cethleann.ManagedFS;
using Cethleann.Unbundler;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Koei.DataExporter
{
    [PublicAPI]
    public static class Program
    {
        public static KoeiDataExporterFlags Flags { get; set; } = new KoeiDataExporterFlags();

        private static void Main(string[] args)
        {
            Flags = CommandLineFlags.ParseFlags<KoeiDataExporterFlags>(CommandLineFlags.PrintHelp, args);

            using var cethleann = new Flayn(Flags.BaseDirectory, GameId.FireEmblemThreeHouses);

            if (Flags.PatchDirectory != null) cethleann.AddPatchFS(Flags.PatchDirectory);

            foreach (var dlcromfs in Flags.DLCDirectories) cethleann.AddDataFS(dlcromfs);
            cethleann.TestDLCSanity();
            cethleann.LoadFileList();
            ExtractAll(Flags.OutputDirectory, cethleann);
        }

        private static void ExtractAll(string romfs, Flayn cethleann)
        {
            for (var index = 0; index < cethleann.EntryCount; index++)
            {
                var data = cethleann.ReadEntry(index);
                var dt = data.Span.GetDataType();
                var ext = UnbundlerLogic.GetExtension(data.Span);
                var pathBase = $@"{romfs}\{cethleann.GetFilename(index, ext, dt)}";
                UnbundlerLogic.TryExtractBlob(pathBase, data, false, false, Flags);
            }
        }
    }
}
