using System;
using System.IO;
using static Cethleann.DataExporter.Program;

namespace Cethleann.Unbundler
{
    static class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                var data = new Memory<byte>(File.ReadAllBytes(arg));
                var pathBase = arg + "_contents";
                TryExtractBlob(pathBase, data, true);
            }
        }
    }
}
