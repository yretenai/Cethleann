using System;
using System.IO;
using System.Linq;
using Cethleann.Structure;
using Cethleann.Tables;
using DragonLib.CLI;
using DragonLib.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cethleann.DataProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Cethleann");
            var flags = CommandLineFlags.ParseFlags<DataExporterFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;


            var typeName = $"{typeof(DataGame).Namespace}.DataStructs.{flags.GameId:G}.{flags.StructName ?? "__UNDEFINED__"}";
            var t = typeof(DataGame).Assembly.GetType(typeName);
            if (t == null || flags.StructName == null)
            {
                if (t == null)
                {
                    Logger.Error("Cethleann", $"Cannot find type {typeName}");
                }
                var ns = $"{typeof(DataGame).Namespace}.DataStructs.{flags.GameId:G}";
                var types = string.Join("\n\t", typeof(DataGame).Assembly.GetTypes().Where(x => x.Namespace == ns).Select(x => x.Name));
                Logger.Info("Cethleann", $"Available types for {flags.GameId:G}:\n\t{types}");
                return;
            }

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (flags.GameId)
            {
                case DataGame.DissidiaNT:
                    ProcessECB(flags, t);
                    break;
                case DataGame.DissidiaOO:
                    ProcessXL20(flags, t);
                    break;
                case DataGame.ThreeHouses:
                    ProcessStruct(flags, t);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ProcessECB(DataExporterFlags flags, Type t)
        {
            foreach (var file in flags.Paths)
            {
                var ecb = new ECB(File.ReadAllBytes(file));
                var ft = Path.ChangeExtension(file, ".json");
                var Entries = ecb.Cast(t);
                File.WriteAllText(ft, JsonConvert.SerializeObject(new
                {
                    t.FullName,
                    Entries
                }, Formatting.Indented, new StringEnumConverter()));
                Logger.Info("ECB", ft);
            }
        }

        private static void ProcessXL20(DataExporterFlags flags, Type t)
        {
            foreach (var file in flags.Paths)
            {
                var xl = new XL20(File.ReadAllBytes(file), t);
                var ft = Path.ChangeExtension(file, ".json");
                File.WriteAllText(ft, JsonConvert.SerializeObject(new
                {
                    xl.UnderlyingType.FullName,
                    xl.Entries
                }, Formatting.Indented, new StringEnumConverter()));
                Logger.Info("XL", ft);
            }
        }

        private static void ProcessStruct(DataExporterFlags flags, Type t)
        {
            foreach (var file in flags.Paths)
            {
                var structTable = new StructTable(File.ReadAllBytes(file));
                var ft = Path.ChangeExtension(file, ".json");
                var Entries = structTable.Cast(t);
                File.WriteAllText(ft, JsonConvert.SerializeObject(new
                {
                    t.FullName,
                    Entries
                }, Formatting.Indented, new StringEnumConverter()));
                Logger.Info("Struct", ft);
            }
        }
    }
}
