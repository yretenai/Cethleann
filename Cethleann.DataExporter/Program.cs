using Cethleann.DataTables;
using Cethleann.Structure.DataStructs;
using System;
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

            var text = new DataTable(DATA0.ReadEntry(DATA1, 0).Span);
            var dh = new DataTable(DATA0.ReadEntry(DATA1, 12).Span);
            var dhChar = new StructTable(dh.Entries.ElementAt(0).Span).Cast<CharacterInfo>();
            var textCh = DATA0Helper.GetTextLocalizationsRoot(text);
            // ExtractTables(romfs, DATA0, DATA1, 0);
            ExtractAll(romfs, DATA0, DATA1);

            return;
        }

#pragma warning disable IDE0051 // Remove unused private members
        static void ExtractTables(string romfs, DATA0 DATA0, Stream DATA1, int index)
        {
            var i = 0;
            var table = new DataTable(DATA0.ReadEntry(DATA1, 0).Span);
            if (!Directory.Exists($@"{romfs}\ex\table\{index:X16}")) Directory.CreateDirectory($@"{romfs}\ex\table\{index:X16}");
            foreach(var entry in table.Entries)
            {
                File.WriteAllBytes($@"{romfs}\ex\table\{index:X16}\{i++:X16}.bin", entry.ToArray());
                Console.WriteLine($@"{romfs}\ex\table\{index:X16}\{i++:X16}.bin");
            }
        }

        static void ExtractAll(string romfs, DATA0 DATA0, Stream DATA1)
        {
            var i = 0;

            if (!Directory.Exists(@$"{romfs}\ex\uncompressed")) Directory.CreateDirectory(@$"{romfs}\ex\uncompressed");
            if (!Directory.Exists(@$"{romfs}\ex\compressed")) Directory.CreateDirectory(@$"{romfs}\ex\compressed");

            foreach (var entry in DATA0.Entries)
            {
                var data = DATA0.ReadEntry(DATA1, entry);
                // File.WriteAllBytes(@$"{romfs}\ex\{(entry.IsCompressed ? "" : "un")}compressed\{i++:X16}.bin", data.ToArray());
                Console.WriteLine(@$"{romfs}\ex\{(entry.IsCompressed ? "" : "un")}compressed\{i:X16}.bin");
            }
        }
#pragma warning restore IDE0051 // Remove unused private members
    }
}
