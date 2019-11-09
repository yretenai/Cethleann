using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Cethleann.DataTables;
using Cethleann.G1;
using Cethleann.Structure.DataStructs;
using static Cethleann.Model.Program;

namespace Cethleann.DataExporter
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Cethleann.DataExporter.exe RomFS output [PatchRomFS [DLCRomFS...]]");
                return;
            }
            var romfs = args.First();
            var output = args.ElementAt(1);
            using var cethleann = new Cethleann(romfs);
            
            if (args.Length > 2)
            {
                cethleann.AddPatchFS(args.ElementAt(2));
            }
            
            foreach (var dlcromfs in args.Skip(3))
            {
                cethleann.AddDataFS(dlcromfs);
            }

            cethleann.ReadEntry(0x026A);

            var bundle = new DataTable(cethleann.ReadEntry(0xE34).Span);
            var model = new G1Model(bundle.Entries.ElementAt(0).Span);
            var texture = new G1TextureGroup(bundle.Entries.ElementAt(1).Span);
            SaveTextures($@"{output}\mdl\tex", texture);
            SaveModel($@"{output}\mdl\{0xE34:X4}.bin", model, "tex");

            TryExtractSCEN($@"{output}\scene", cethleann.ReadEntry(0x2BA));

            var text = new DataTable(cethleann.ReadEntry(0).Span);
            var dh = new DataTable(cethleann.ReadEntry(12).Span);
            var character =new StructTable(dh.Entries.ElementAt(0).Span).Cast<CharacterInfo>();
            var texts = text.Entries.Select(entry => new DataTable(entry.Span)).Select(table => table.Entries.Select(entry => new TextLocalization(entry.Span)).ToArray()).ToArray();
            // ExtractTables(romfs, cethleann, 0);
            ExtractAll(output, cethleann);
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static void ExtractTables(string romfs, Cethleann cethleann, int index)
        {
            var i = 0;
            var table = new DataTable(cethleann.ReadEntry(index).Span);
            if (!Directory.Exists($@"{romfs}\table\{index:X4}")) Directory.CreateDirectory($@"{romfs}\table\{index:X4}");

            foreach (var entry in table.Entries)
            {
                File.WriteAllBytes($@"{romfs}\table\{index:X4}\{i++:X4}.bin", entry.ToArray());
                Console.WriteLine($@"{romfs}\table\{index:X4}\{i++:X4}.bin");
            }
        }

        private static void ExtractAll(string romfs, Cethleann cethleann)
        {
            var i = 0;

            if (!Directory.Exists($@"{romfs}\romfs")) Directory.CreateDirectory($@"{romfs}\romfs");

            for (var index = 0; index < cethleann.EntryCount; index++)
            {
                var data = cethleann.ReadEntry(index);
                var pathBase = $@"{romfs}\romfs\{i++:X4}";
                var ext = data.Span.GetDataType().GetExtension();
                Console.WriteLine($@"{pathBase}.{ext}");
                if (data.Length == 0)
                {
                    Console.WriteLine($"{pathBase}.{ext} is zero!");
                    continue;
                }

                if (!data.Span.IsKnown() && data.Span.IsDataTable())
                    if (TryExtractDataTable($"{pathBase}.datatable", data))
                        continue;
                if (data.Span.GetDataType() == DataType.SCEN)
                    if (TryExtractSCEN($"{pathBase}.scene", data))
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

        private static bool TryExtractSCEN(string pathBase, Memory<byte> data)
        {
            try
            {
                var blobs = new SCEN(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries);
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Failed unpacking SCEN, {e.Message}!");
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
                var blobBase = $@"{pathBase}\{j++:X4}";
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
#pragma warning restore IDE0051 // Remove unused private members
    }
}
