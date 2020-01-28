using System;
using System.IO;
using Cethleann;
using Cethleann.Ninja;
using DragonLib.IO;

namespace Ninja.Gz
{
    static class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                Logger.Info("NINJA", arg);
                var data = new Span<byte>(File.ReadAllBytes(arg));
                if (arg.EndsWith(".dz"))
                {
                    var buffer = DzCompression.Decompress(data);
                    var newName = Path.GetFileNameWithoutExtension(arg);
                    if (newName == Path.GetFileNameWithoutExtension(newName)) newName += $".{buffer.GetDataType().GetExtension()}";
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(arg), newName), buffer.ToArray());
                }
                else
                {
                    File.WriteAllBytes(arg + ".dz", Compression.Compress(data).ToArray());
                }
            }
        }
    }
}
