using System.IO;
using System.Linq;
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

            using var flayn = new Flayn(Flags.GameId)
            {
                PrefixLinkData = Flags.UseLinkdataPrefix
            };

            flayn.AddLinkFS(Flags.BaseDirectory, Flags.IDXHint, Flags.BINHint);
            if (Flags.PatchDirectory != null && Directory.Exists(Flags.PatchDirectory)) flayn.AddPatchFS(Flags.PatchDirectory);
            foreach (var dlcromfs in Flags.DLCDirectories.Where(Directory.Exists)) flayn.AddLinkFS(dlcromfs, Flags.IDXHint, Flags.BINHint);
            flayn.LoadFileList(Flags.FileList);
            ExtractAll(Flags.OutputDirectory, flayn);
        }

        private static void ExtractAll(string romfs, IManagedFS cethleann)
        {
            for (var index = 0; index < cethleann.EntryCount; index++)
            {
                var data = cethleann.ReadEntry(index).Span;
                var dt = data.GetDataType();
                var ext = UnbundlerLogic.GetExtension(data);
                var pathBase = $@"{romfs}\{cethleann.GetFilename(index, ext, dt)}";
                UnbundlerLogic.TryExtractBlob(pathBase, data, false, Flags);
            }
        }
    }
}
