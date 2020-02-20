using System;
using System.IO;
using System.Linq;
using Cethleann;
using Cethleann.Koei;
using Cethleann.KTID;
using Cethleann.Structure;
using DragonLib.CLI;
using DragonLib.IO;

namespace Nyotengu.Database
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Nyotengu");
            var flags = CommandLineFlags.ParseFlags<DatabaseFlags>(CommandLineFlags.PrintHelp, args);

            foreach (var path in flags.Paths)
            {
                Span<byte> buffer = File.ReadAllBytes(path);
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (buffer.GetDataType())
                {
                    case DataType.KOD:
                        break;
                    case DataType.NAME:
                    {
                        var name = new NAME(buffer);
                        foreach (var (entry, strings) in name.Entries)
                        {
                            var filename = name.NameMap[entry.KTID];
                            var text = $"{entry.KTID:x8},{strings.ElementAtOrDefault(0) ?? "null"},{filename}";
                            if (strings.Length > 1)
                            {
                                text += $",{RDB.Hash(strings[1]):x8},{strings[1]}";
                                if (strings.Length > 2) text += string.Join(",", strings.Skip(2));
                            }

                            Console.WriteLine(text);
                        }

                        break;
                    }
                    default:
                        Logger.Error("Nyotengu", $"Format for {path} is unknown!");
                        break;
                }
            }
        }
    }
}
