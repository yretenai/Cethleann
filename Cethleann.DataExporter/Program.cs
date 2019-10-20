using Cethleann.DataTables;
using Cethleann.G1;
using Cethleann.Structure.DataStructs;
using DragonLib;
using DragonLib.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cethleann.DataExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var romfs = args.Last();
            var DATA0 = new DATA0(@$"{romfs}\DATA0.bin");
            using var DATA1 = File.OpenRead(@$"{romfs}\DATA1.bin");

            var bundle = new DataTable(DATA0.ReadEntry(DATA1, 0xE34).Span);
            var texture = new G1TextureGroup(bundle.Entries.ElementAt(1).Span);
            var i = 0;
            if (!Directory.Exists(@$"{romfs}\ex\tex")) Directory.CreateDirectory(@$"{romfs}\ex\tex");
            foreach (var (usage, header, extra, blob) in texture.Textures)
            {
                var (width, height, mips, format) = G1TextureGroup.UnpackWHM(header);
                var data = DXGI.DecompressDXGIFormat(blob.Span, width, height, format);
                i += 1;
                TiffImage.WriteTiff($@"{romfs}\ex\tex\{i:X16}.tif", data, width, height);
                File.WriteAllBytes($@"{romfs}\ex\tex\{i:X16}.dds", DXGI.BuildDDS(format, mips, width, height, blob.Span).ToArray());
            }
            var text = new DataTable(DATA0.ReadEntry(DATA1, 0).Span);
            var dh = new DataTable(DATA0.ReadEntry(DATA1, 12).Span);
            var dhChar = new StructTable(dh.Entries.ElementAt(0).Span).Cast<CharacterInfo>();
            var textCh = text.GetTextLocalizationsRoot();
            // ExtractTables(romfs, DATA0, DATA1, 0);
            // ExtractAll(romfs, DATA0, DATA1);
            // File.WriteAllText($@"{romfs}\ex\magic.txt", string.Join('\n', MagicValues.Select(x => x.ToString("X"))));
            return;
        }

#pragma warning disable IDE0051 // Remove unused private members
        static void ExtractTables(string romfs, DATA0 DATA0, Stream DATA1, int index)
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

        static HashSet<DataType> MagicValues { get; set; } = new HashSet<DataType>();

        static void ExtractAll(string romfs, DATA0 DATA0, Stream DATA1)
        {
            var i = 0;

            if (!Directory.Exists(@$"{romfs}\ex\uncompressed")) Directory.CreateDirectory(@$"{romfs}\ex\uncompressed");
            if (!Directory.Exists(@$"{romfs}\ex\compressed")) Directory.CreateDirectory(@$"{romfs}\ex\compressed");

            foreach (var entry in DATA0.Entries)
            {
                var data = DATA0.ReadEntry(DATA1, entry);
                string ext;
                var pathBase = @$"{romfs}\ex\{(entry.IsCompressed ? "" : "un")}compressed\{i++:X16}";
                ext = data.Span.GetDataType().GetExtension();
                Console.WriteLine(@$"{pathBase}.{ext}");
                if (data.Length == 0)
                {
                    Console.WriteLine($"{pathBase}.{ext} is zero!");
                    continue;
                }
                if (!data.Span.IsKnown() && data.Span.IsDataTable())
                {
                    TryExtractDataTable(pathBase, data);
                }
                else
                {
                    MagicValues.Add(data.Span.GetDataType());
                }
                File.WriteAllBytes(@$"{pathBase}.{ext}", data.ToArray());
            }
        }

        private static bool TryExtractDataTable(string pathBase, Memory<byte> data)
        {
            try
            {
                var blobs = new DataTable(data.Span);
                if (!Directory.Exists(pathBase))
                {
                    Directory.CreateDirectory(pathBase);
                }

                var j = 0;
                foreach (var datablob in blobs.Entries)
                {
                    var blobBase = @$"{pathBase}\{j++:X16}";
                    var ext = datablob.Span.GetDataType().GetExtension();
                    Console.WriteLine(@$"{blobBase}.{ext}");
                    if (data.Length == 0)
                    {
                        Console.WriteLine($"{blobBase}.{ext} is zero!");
                        continue;
                    }
                    if (!datablob.Span.IsKnown() && datablob.Span.IsDataTable())
                    {
                        if (TryExtractDataTable(blobBase, datablob)) continue;
                    }
                    MagicValues.Add(datablob.Span.GetDataType());
                    File.WriteAllBytes(@$"{blobBase}.{ext}", datablob.ToArray());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(@$"Failed unpacking DataTable, {e.Message}!");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);
                return false;
            }
            return true;
        }
#pragma warning restore IDE0051 // Remove unused private members
    }
}
