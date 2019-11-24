using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Cethleann.DataTables;
using Cethleann.G1;
using static Koei.DataExporter.Program;

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
                    var ext = Path.GetExtension(originalName).ToLower();
                    switch (ext)
                    {
                        case ".datatable":
                        case ".bin":
                            TryPackDatatable(Path.ChangeExtension(originalName, "mod.datatable"), arg);
                            break;
                        case ".g1m":
                            TryPackG1M(Path.ChangeExtension(originalName, "mod.g1m"), arg);
                            break;
                    }

                    continue;
                }

                var data = new Memory<byte>(File.ReadAllBytes(arg));
                var pathBase = arg + "_contents";
                TryExtractBlob(pathBase, data, true, true);
            }
        }

        public static void TryPackG1M(string path, string dir)
        {
            var files = Directory.GetFiles(dir).OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x).Split('_')[0], NumberStyles.Integer)).ToArray();
            var model = new IktglModel();
            foreach (var file in files) model.SectionRoot.Sections.Add(new Memory<byte>(File.ReadAllBytes(file)));
            File.WriteAllBytes(path, model.WriteFromRoot().ToArray());
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
