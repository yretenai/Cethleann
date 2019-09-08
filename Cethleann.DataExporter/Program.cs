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
    }
}
