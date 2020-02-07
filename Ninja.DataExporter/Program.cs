using System.IO;
using Cethleann;
using Cethleann.ManagedFS;
using Cethleann.ManagedFS.Support;
using Cethleann.Structure;
using Cethleann.Unbundler;
using DragonLib.CLI;
using DragonLib.IO;

namespace Ninja.DataExporter
{
    static class Program
    {
        public static NinjaDataExporterFlags Flags { get; set; } = new NinjaDataExporterFlags();

        static void Main(string[] args)
        {
            Logger.PrintVersion("NINJA");
            Flags = CommandLineFlags.ParseFlags<NinjaDataExporterFlags>(CommandLineFlags.PrintHelp, args);


            var settings = Flags.GameId switch
            {
                DataGame.DissidiaNT => (YshtolaSettings) new YshtolaDissidiaSettings(),
                DataGame.VenusVacation => new YshtolaVenusVacationSettings(),
                _ => null
            };
            IManagedFS fs;
            if (settings == null)
            {
                var mitsunari = new Mitsunari(Flags.GameId);
                mitsunari.AddDataFS(Flags.RootDirectory);
                mitsunari.LoadFileList();
                fs = mitsunari;
            }
            else
            {
                var yshtola = new Yshtola(Flags.GameId, Flags.RootDirectory, settings);

                if (!Directory.Exists(Flags.OutputDirectory)) Directory.CreateDirectory(Flags.OutputDirectory);

                for (var index = 0; index < yshtola.Tables.Count; index++)
                {
                    var table = yshtola.Tables[index];
                    var type = Path.GetDirectoryName(yshtola.Settings.TableNames[index]);
                    var name = $"manifest-{type ?? "COMMON"}.{Flags.GameId.ToString("X").ToLower()}";
                    File.WriteAllBytes(Path.Combine(Flags.OutputDirectory, name), table.Buffer.ToArray());
                }

                if (Flags.ManifestOnly) return;
                fs = yshtola;
            }

            ExtractAll(Flags.OutputDirectory, fs);
            fs.Dispose();
        }

        private static void ExtractAll(string romfs, IManagedFS fs)
        {
            for (var index = 0; index < fs.EntryCount; index++)
            {
                var data = fs.ReadEntry(index);
                var dt = data.Span.GetDataType();
                var ext = UnbundlerLogic.GetExtension(data.Span);
                var filepath = fs.GetFilename(index, ext, dt);
                while (filepath.StartsWith("\\") || filepath.StartsWith("/")) filepath = filepath.Substring(1);
                var pathBase = $@"{romfs}\{filepath}";
                UnbundlerLogic.TryExtractBlob(pathBase, data, false, Flags);
            }
        }
    }
}
