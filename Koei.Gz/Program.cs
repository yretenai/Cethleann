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
                    var buffer = Compression.Decompress(data);
                    var newName = Path.GetFileNameWithoutExtension(arg);
                    if (newName == Path.GetFileNameWithoutExtension(newName)) newName += $".{buffer.GetDataType().GetExtension()}";
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(arg), newName), buffer.ToArray());
                }
                else
                {
                    File.WriteAllBytes(arg + ".gz", Compression.Compress(data).ToArray());
                }
            }
        }
    }
}
