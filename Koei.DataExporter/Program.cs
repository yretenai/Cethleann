using Cethleann;
using Cethleann.ManagedFS;
using Cethleann.Unbundler;
using DragonLib.CLI;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Koei.DataExporter
{
    [PublicAPI]
    public static class Program
    {
        public static KoeiDataExporterFlags Flags { get; set; } = new KoeiDataExporterFlags();

        private static void Main(string[] args)
        {
            Logger.PrintVersion("KTGL");
            Flags = CommandLineFlags.ParseFlags<KoeiDataExporterFlags>(CommandLineFlags.PrintHelp, args);

            using var flayn = new Flayn(Flags.BaseDirectory, Flags.GameId);

            if (Flags.PatchDirectory != null) flayn.AddPatchFS(Flags.PatchDirectory);

            foreach (var dlcromfs in Flags.DLCDirectories) flayn.AddDataFS(dlcromfs);
#if DEBUG
            flayn.TestDLCSanity();
#endif
            flayn.LoadFileList();
            ExtractAll(Flags.OutputDirectory, flayn);
        }

        private static void ExtractAll(string romfs, IManagedFS cethleann)
        {
            for (var index = 0; index < cethleann.EntryCount; index++)
            {
                var data = cethleann.ReadEntry(index);
                var dt = data.Span.GetDataType();
                var ext = UnbundlerLogic.GetExtension(data.Span);
                var pathBase = $@"{romfs}\{cethleann.GetFilename(index, ext, dt)}";
                UnbundlerLogic.TryExtractBlob(pathBase, data, false, Flags);
            }
        }
    }
}
