using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Cethleann.Koei;
using Cethleann.ManagedFS;
using DragonLib;
using DragonLib.CLI;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Softness.Hash
{
    [PublicAPI]
    public static class Program
    {
        private static void Main(string[] args)
        {
            var flags = CommandLineFlags.ParseFlags<SoftnessHashFlags>(CommandLineFlags.PrintHelp, args);
            Logger.Assert(RDB.Hash("HEL_COS_103", "G1M") == 0x57DF4CFCu, "Sanity Check: RDB.Hash('HEL_COS_103', 'G1M') == 0x57DF4CFC");

            HashSet<uint> hashes;
            if (flags.NoRDB)
            {
                hashes = File.ReadAllLines(flags.GameDirectory).Select(uint.Parse).ToHashSet();
                if (hashes.Count == 0) Logger.Error("SOFT", "Could not parse hashes");
            }
            else
            {
                using var nyotengu = new Nyotengu(flags.GameId);
                foreach (var rdb in Directory.GetFiles(flags.GameDirectory, "*.rdb")) nyotengu.AddDataFS(rdb);
                nyotengu.LoadFileList();
                var targetId = uint.Parse(flags.TypeId, NumberStyles.HexNumber);
                hashes = nyotengu.RDBs.SelectMany(x => x.Entries.Select(y => y.entry)).Where(x => x.TypeId == targetId).Select(x => x.FileId).ToHashSet();
                if (hashes.Count == 0) Logger.Error("SOFT", $"Could not find hashes with type id {targetId}");
            }

            var targetName = flags.TypeName.ToUpper();
            var charMap = Encoding.ASCII.GetBytes("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-");

            // Based off https://github.com/jwoschitz/Brutus, see BRUTUS_LICENSE.txt
            Span<byte> generated = new byte[flags.Max];
            var pos = 0;
            while (pos <= flags.Max)
            {
                if (pos == 0)
                {
                    for (var i = 0; i < flags.Min; ++i) generated[i] = charMap[0];

                    pos = flags.Min;
                }
                else
                {
                    var generatedIndex = pos - 1;
                    var charIndex = Array.IndexOf(charMap, generated[generatedIndex]);
                    while (charIndex == charMap.Length - 1)
                    {
                        generated[generatedIndex] = charMap[0];
                        if (generatedIndex == 0)
                        {
                            generatedIndex = pos++;
                            generated[generatedIndex] = charMap[0];
                        }
                        else
                        {
                            generatedIndex--;
                            charIndex = Array.IndexOf(charMap, generated[generatedIndex]);
                        }
                    }

                    generated[generatedIndex] = charMap[++charIndex];
                }


                var hash = RDB.Hash(flags.Prefix + generated.Slice(0, pos).ReadString(), targetName, flags.GlobalPrefix);
                if (hashes.Contains(hash)) Console.WriteLine($"{hash:x8},{generated.ReadString()}");
            }
        }
    }
}
