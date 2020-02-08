using Cethleann;
using Cethleann.ManagedFS;
using Cethleann.Unbundler;
using DragonLib.CLI;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Omega.DataExporter
{
    [PublicAPI]
    public static class Program
    {
        public static OmegaDataExporterFlags Flags { get; set; } = new OmegaDataExporterFlags();

        private static void Main(string[] args)
        {
            Logger.PrintVersion("OMEGA");
            Flags = CommandLineFlags.ParseFlags<OmegaDataExporterFlags>(CommandLineFlags.PrintHelp, args);

            using var leonhart = new Leonhart(Flags.GameId);
            foreach (var romfs in Flags.Directories) leonhart.AddDataFS(romfs);
            leonhart.LoadFileList();
            ExtractAll(Flags.OutputDirectory, leonhart);
        }

        private static void ExtractAll(string romfs, IManagedFS leonhart)
        {
            for (var index = 0; index < leonhart.EntryCount; index++)
            {
                var data = leonhart.ReadEntry(index).Span;
                var dt = data.GetDataType();
                var ext = UnbundlerLogic.GetExtension(data);
                var pathBase = $@"{romfs}\{leonhart.GetFilename(index, ext, dt)}";
                UnbundlerLogic.TryExtractBlob(pathBase, data, false, Flags);
            }
        }
    }
}
