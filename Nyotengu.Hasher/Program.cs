using System;
using System.IO;
using Cethleann.Koei;
using DragonLib.CLI;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Nyotengu.Hasher
{
    [PublicAPI]
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Softness");
            var flags = CommandLineFlags.ParseFlags<SoftnessHasherFlags>(CommandLineFlags.PrintHelp, args);
            foreach (var str in flags.Strings) Console.WriteLine($"{(flags.Raw ? RDB.Hash(str) : RDB.Hash(Path.GetFileNameWithoutExtension(str), flags.Format ?? Path.GetExtension(str).Substring(1).ToUpper(), flags.Prefix)):x8},{str}");
        }
    }
}
