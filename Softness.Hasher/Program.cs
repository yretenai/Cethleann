using System;
using Cethleann.Koei;
using DragonLib.CLI;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Softness.Hasher
{
    [PublicAPI]
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("SOFT");
            var flags = CommandLineFlags.ParseFlags<SoftnessHasherFlags>(CommandLineFlags.PrintHelp, args);
            foreach (var str in flags.Strings) Console.WriteLine($"{(flags.Raw ? RDB.Hash(str) : RDB.Hash(str, flags.Format, flags.Prefix)):x8},{str}");
        }
    }
}
