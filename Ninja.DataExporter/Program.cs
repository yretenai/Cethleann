using System;
using System.IO;
using Cethleann.ManagedFS;

namespace Ninja.DataExporter
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Ninja.DataExporter.exe output install_directory");
                return;
            }

            var output = args[0];
            // var _ = args[1].ToLower();
            var yshtola = new Yshtola(args[1], new DissidiaSettings());

            foreach (var entry in yshtola)
            {
                try
                {
                    var (data, filepath) = yshtola.ReadEntry(entry);
                    if (data.Length == 0)
                    {
                        Console.WriteLine($"{entry} is zero!");
                        continue;
                    }

                    while (filepath.StartsWith("\\") || filepath.StartsWith("/")) filepath = filepath.Substring(1);

                    var path = Path.Combine(output, filepath);
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllBytes(path, data.ToArray());
                    Console.WriteLine(path);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
