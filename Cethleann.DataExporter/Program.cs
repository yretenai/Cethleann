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

            var text = new DataTable(DATA0.ReadEntry(DATA1, 0));
            var dh = new DataTable(DATA0.ReadEntry(DATA1, 12));
            var dhChar = new StructTable(dh.Entries.ElementAt(0).Span).Cast<CharacterInfo>();
            var textCh = new DataTable(text.Entries.ElementAt(0).Span);

            // ExtractAll(romfs, DATA0, DATA1);

            return;
        }

#pragma warning disable IDE0051 // Remove unused private members
        static void ExtractAll(string romfs, DATA0 DATA0, Stream DATA1)
        {
            var i = 0;

            if (!Directory.Exists(@$"{romfs}\ex\uncompressed")) Directory.CreateDirectory(@$"{romfs}\ex\uncompressed");
            if (!Directory.Exists(@$"{romfs}\ex\compressed")) Directory.CreateDirectory(@$"{romfs}\ex\compressed");

            foreach (var entry in DATA0.Entries)
            {
                using var data = DATA0.ReadEntry(DATA1, entry);
                File.WriteAllBytes(@$"{romfs}\ex\{(entry.IsCompressed ? "" : "un")}compressed\{i++:X16}.bin", data.ToArray());
                Console.WriteLine(@$"{romfs}\ex\{(entry.IsCompressed ? "" : "un")}compressed\{i:X16}.bin");
            }
        }
#pragma warning restore IDE0051 // Remove unused private members
    }
}
