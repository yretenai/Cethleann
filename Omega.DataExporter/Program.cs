using Cethleann;
using Cethleann.ManagedFS;
using Cethleann.Unbundler;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Omega.DataExporter
{
    [PublicAPI]
    public static class Program
    {
        public static OmegaDataExporterFlags Flags { get; set; } = new OmegaDataExporterFlags();

        private static void Main(string[] args)
        {
            Flags = CommandLineFlags.ParseFlags<OmegaDataExporterFlags>(CommandLineFlags.PrintHelp, args);

            using var leonhart = new Leonhart(Flags.GameId);
            foreach (var romfs in Flags.Directories) leonhart.AddDataFS(romfs);
            leonhart.LoadFileList();
            ExtractAll(Flags.OutputDirectory, leonhart);
        }

        private static void ExtractAll(string romfs, Leonhart leonhart)
        {
            for (var index = 0; index < leonhart.EntryCount; index++)
            {
                var data = leonhart.ReadEntry(index);
                var dt = data.Span.GetDataType();
                var ext = UnbundlerLogic.GetExtension(data.Span);
                var pathBase = $@"{romfs}\{leonhart.GetFilename(index, ext, dt)}";
                UnbundlerLogic.TryExtractBlob(pathBase, data, false, Flags);
            }
        }
    }
}
