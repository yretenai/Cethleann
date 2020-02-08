using System;
using System.IO;
using Cethleann;
using Cethleann.Koei;
using DragonLib.IO;

namespace Koei.Gz
{
    static class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                Logger.PrintVersion("KTGL");
                Logger.Info("KTGL", arg);
                var data = new Span<byte>(File.ReadAllBytes(arg));
                if (arg.EndsWith(".gz"))
                {
                    if (data[4] == 0x78)
                    {
                        var newName = Path.GetFileNameWithoutExtension(arg);
                        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(arg), newName), Compression.Decompress(data, -1, 1).ToArray());
                    }
                    else
                    {
                        var buffer = TableCompression.Decompress(data, true);
                        if (buffer.Length == 0) buffer = data;
                        var newName = Path.GetFileNameWithoutExtension(arg);
                        if (newName == Path.GetFileNameWithoutExtension(newName)) newName += $".{buffer.GetDataType().GetExtension()}";
                        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(arg), newName), buffer.ToArray());
                    }
                }
                else
                {
                    File.WriteAllBytes(arg + ".gz", TableCompression.Compress(data).ToArray());
                }
            }
        }
    }
}
