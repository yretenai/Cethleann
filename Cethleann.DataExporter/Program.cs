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
    public static class Program
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

            if (args.Length > 3 && Directory.Exists(args.ElementAt(2))) cethleann.AddPatchFS(args.ElementAt(2));

            foreach (var dlcromfs in args.Skip(3)) cethleann.AddDataFS(dlcromfs);

#if DEBUG
            cethleann.ReadEntry(0x026A);
            var bundle = new DataTable(cethleann.ReadEntry(0xE34).Span);
            var model = new G1Model(bundle.Entries.ElementAt(0).Span);
            var texture = new G1TextureGroup(bundle.Entries.ElementAt(1).Span);
            SaveTextures($@"{output}\mdl\tex", texture);
            SaveModel($@"{output}\mdl\{0xE34:X4}.gltf", model, "tex");
            TryExtractBlob($@"{output}\mdlktest", cethleann.ReadEntry(0x04CD));

            var text = new DataTable(cethleann.ReadEntry(0).Span);
            var dh = new DataTable(cethleann.ReadEntry(12).Span);
            var character = new StructTable(dh.Entries.ElementAt(0).Span).Cast<CharacterInfo>();
            var texts = text.Entries.Select(entry => new DataTable(entry.Span)).Select(table => table.Entries.Select(entry => new TextLocalization(entry.Span)).ToArray()).ToArray();
            // ExtractTables(romfs, cethleann, 0);
#endif
            ExtractAll(output, cethleann);
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static void ExtractTables(string romfs, Cethleann cethleann, int index)
        {
            var table = new DataTable(cethleann.ReadEntry(index).Span);
            if (!Directory.Exists($@"{romfs}\table\{index:X4}")) Directory.CreateDirectory($@"{romfs}\table\{index:X4}");

            for (var index1 = 0; index1 < table.Entries.Count; index1++)
            {
                var entry = table.Entries[index1];
                File.WriteAllBytes($@"{romfs}\table\{index:X4}\{index1:X4}.bin", entry.ToArray());
                Console.WriteLine($@"{romfs}\table\{index:X4}\{index1:X4}.bin");
            }
        }

        private static void ExtractAll(string romfs, Cethleann cethleann)
        {
            if (!Directory.Exists($@"{romfs}\romfs")) Directory.CreateDirectory($@"{romfs}\romfs");

            for (var index = 0; index < cethleann.EntryCount; index++)
            {
                var data = cethleann.ReadEntry(index);
                var pathBase = $@"{romfs}\romfs\{index:X4}";
                TryExtractBlob(pathBase, data);
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

        public static void TryExtractBlobs(string pathBase, List<Memory<byte>> blobs, bool allTypes = false)
        {
            for (var index = 0; index < blobs.Count; index++)
            {
                var datablob = blobs[index];
                TryExtractBlob($@"{pathBase}\{index:X4}", datablob, allTypes);
            }
        }

        public static int TryExtractBlob(string blobBase, Memory<byte> datablob, bool allTypes = false)
        {
            var ext = datablob.Span.GetDataType().GetExtension();
            if (datablob.Length == 0)
            {
                Console.WriteLine($"{blobBase}.{ext} is zero!");
                return 0;
            }

            if (!datablob.Span.IsKnown() && datablob.Span.IsDataTable())
                if (TryExtractDataTable($"{blobBase}.datatable", datablob))
                    return 1;
            if (datablob.Span.GetDataType() == DataType.SCEN)
                if (allTypes && TryExtractSCEN($"{blobBase}.scen", datablob))
                    return 1;
                else
                    ext = "scen";
            if (datablob.Span.IsBundle())
                if (allTypes && TryExtractBundle($"{blobBase}.bundle", datablob))
                    return 1;
                else
                    ext = "bundle";
            if (datablob.Span.GetDataType() == DataType.MDLK)
                if (allTypes && TryExtractModelGroup($"{blobBase}.mdlk", datablob))
                    return 1;
                else
                    ext = "mdlk";

            Console.WriteLine($@"{blobBase}.{ext}");

            var basedir = Path.GetDirectoryName(blobBase);
            if (!Directory.Exists(basedir)) Directory.CreateDirectory(basedir);

            File.WriteAllBytes($@"{blobBase}.{ext}", datablob.ToArray());
            return 2;
        }
#pragma warning restore IDE0051 // Remove unused private members
    }
}
