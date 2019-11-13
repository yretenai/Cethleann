using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Cethleann.DataTables;
using static Cethleann.DataExporter.Program;

namespace Cethleann.Unbundler
{
    static class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
                if (Directory.Exists(arg) && arg.EndsWith("_contents"))
                {
                    var originalName = arg.Substring(0, arg.Length - 9);
                    // try to repackage the blob.
                    if (arg.EndsWith(".datatable") || arg.EndsWith(".bin")) TryPackDatatable(Path.ChangeExtension(originalName, "mod.bin"), arg);
                    continue;
                }

                var data = new Memory<byte>(File.ReadAllBytes(arg));
                var pathBase = arg + "_contents";
                TryExtractBlob(pathBase, data, true);
            }
        }

        private static void TryPackDatatable(string path, string dir)
        {
            var files = Directory.GetFiles(dir).OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x), NumberStyles.HexNumber)).ToArray();
            var dataTable = new DataTable
            {
                Entries = files.Select(x => new Memory<byte>(File.ReadAllBytes(x))).ToList()
            }.Write().ToArray();
            File.WriteAllBytes(path, dataTable);
        }
    }
}
