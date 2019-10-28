using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cethleann.DataTables;
using Cethleann.G1;
using Cethleann.Structure.DataStructs;
using DragonLib.Imaging;
using DragonLib.Imaging.DXGI;

namespace Cethleann.DataExporter
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var romfs = args.Last();
            var DATA0 = new DATA0($@"{romfs}\DATA0.bin");
            using var DATA1 = File.OpenRead($@"{romfs}\DATA1.bin");

            var bundle = new DataTable(DATA0.ReadEntry(DATA1, 0xE34).Span);
            var model = new G1Model(bundle.Entries.ElementAt(0).Span);
            if (!Directory.Exists($@"{romfs}\ex\mdl")) Directory.CreateDirectory($@"{romfs}\ex\mdl");
            var gltf = model.ExportMeshes($@"{romfs}\ex\mdl\model.bin", "model.bin", 0, 0, true);
            using var file = File.OpenWrite($@"{romfs}\ex\mdl\model.gltf");
            file.SetLength(0);
            using var writer = new StreamWriter(file);
            gltf.Serialize(writer);
            var texture = new G1TextureGroup(bundle.Entries.ElementAt(1).Span);
            SaveTextures($@"{romfs}\ex\tex", texture);

            var text = new DataTable(DATA0.ReadEntry(DATA1, 0).Span);
            var dh = new DataTable(DATA0.ReadEntry(DATA1, 12).Span);
            _ = new StructTable(dh.Entries.ElementAt(0).Span).Cast<CharacterInfo>();
            _ = text.GetTextLocalizationsRoot();
            // ExtractTables(romfs, DATA0, DATA1, 0);
            // ExtractAll(romfs, DATA0, DATA1);
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static void ExtractTables(string romfs, DATA0 DATA0, Stream DATA1, int index)
        {
            var i = 0;
            var table = new DataTable(DATA0.ReadEntry(DATA1, index).Span);
            if (!Directory.Exists($@"{romfs}\ex\table\{index:X16}")) Directory.CreateDirectory($@"{romfs}\ex\table\{index:X16}");

            foreach (var entry in table.Entries)
            {
                File.WriteAllBytes($@"{romfs}\ex\table\{index:X16}\{i++:X16}.bin", entry.ToArray());
                Console.WriteLine($@"{romfs}\ex\table\{index:X16}\{i++:X16}.bin");
            }
        }

        private static void ExtractAll(string romfs, DATA0 DATA0, Stream DATA1)
        {
            var i = 0;

            if (!Directory.Exists($@"{romfs}\ex\uncompressed")) Directory.CreateDirectory($@"{romfs}\ex\uncompressed");

            if (!Directory.Exists($@"{romfs}\ex\compressed")) Directory.CreateDirectory($@"{romfs}\ex\compressed");

            foreach (var entry in DATA0.Entries)
            {
                var data = DATA0.ReadEntry(DATA1, entry);
                var pathBase = $@"{romfs}\ex\{(entry.IsCompressed ? "" : "un")}compressed\{i++:X16}";
                var ext = data.Span.GetDataType().GetExtension();
                Console.WriteLine($@"{pathBase}.{ext}");
                if (data.Length == 0)
                {
                    Console.WriteLine($"{pathBase}.{ext} is zero!");
                    continue;
                }

                if (!data.Span.IsKnown() && data.Span.IsDataTable())
                    if (TryExtractDataTable($"{pathBase}.datatable", data))
                        continue; /*
                if (data.Span.IsBundle())
                    if (TryExtractBundle($"{pathBase}.bundle", data))
                        continue;
                if (data.Span.GetDataType() == DataType.MDLK)
                    if (TryExtractModelGroup($"{pathBase}.mdlk", data))
                        continue; */
                File.WriteAllBytes($@"{pathBase}.{ext}", data.ToArray());
            }
        }

        private static bool TryExtractDataTable(string pathBase, Memory<byte> data)
        {
            try
            {
                var blobs = new DataTable(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries);
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Failed unpacking DataTable, {e.Message}!");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractBundle(string pathBase, Memory<byte> data)
        {
            try
            {
                var blobs = new Bundle(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries);
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Failed unpacking Bundle, {e.Message}!");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractModelGroup(string pathBase, Memory<byte> data)
        {
            try
            {
                var blobs = new G1ModelGroup(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries);
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Failed unpacking ModelGroup, {e.Message}!");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static void TryExtractBlobs(string pathBase, List<Memory<byte>> blobs)
        {
            var j = 0;
            foreach (var datablob in blobs)
            {
                var blobBase = $@"{pathBase}\{j++:X16}";
                var ext = datablob.Span.GetDataType().GetExtension();
                Console.WriteLine($@"{blobBase}.{ext}");
                if (datablob.Length == 0)
                {
                    Console.WriteLine($"{blobBase}.{ext} is zero!");
                    continue;
                }

                if (!datablob.Span.IsKnown() && datablob.Span.IsDataTable())
                    if (TryExtractDataTable($"{blobBase}.datatable", datablob))
                        continue; /*
                if (datablob.Span.IsBundle())
                    if (TryExtractBundle($"{blobBase}.bundle", datablob))
                        continue;
                if (datablob.Span.GetDataType() == DataType.MDLK)
                    if (TryExtractModelGroup($"{blobBase}.mdlk", datablob))
                        continue; */

                if (!Directory.Exists(pathBase)) Directory.CreateDirectory(pathBase);

                File.WriteAllBytes($@"{blobBase}.{ext}", datablob.ToArray());
            }
        }

        private static void SaveTextures(string pathBase, G1TextureGroup group)
        {
            var i = 0;
            if (!Directory.Exists(pathBase)) Directory.CreateDirectory(pathBase);

            foreach (var (_, header, _, blob) in group.Textures)
            {
                var (width, height, mips, format) = G1TextureGroup.UnpackWHM(header);
                var data = DXGI.DecompressDXGIFormat(blob.Span, width, height, format);
                i += 1;
                if (!TiffImage.WriteTiff($@"{pathBase}\{i:X16}.tif", data, width, height)) File.WriteAllBytes($@"{pathBase}\{i:X16}.dds", DXGI.BuildDDS(format, mips, width, height, blob.Span).ToArray());
            }
        }
#pragma warning restore IDE0051 // Remove unused private members
    }
}
