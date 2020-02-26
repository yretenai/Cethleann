using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cethleann;
using Cethleann.Archive;
using Cethleann.KTID;
using Cethleann.Structure;
using Cethleann.Structure.KTID;
using DragonLib.CLI;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Nyotengu.Database
{
    [PublicAPI]
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Nyotengu");
            var flags = CommandLineFlags.ParseFlags<DatabaseFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;

            Span<byte> buffer = File.ReadAllBytes(flags.Path);
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (buffer.GetDataType())
            {
                case DataType.OBJDB:
                {
                    var ndb = new NDB();
                    if (!string.IsNullOrWhiteSpace(flags.NDBPath) && File.Exists(flags.NDBPath)) ndb = new NDB(File.ReadAllBytes(flags.NDBPath));
                    ProcessOBJDB(buffer, ndb, flags);
                    break;
                }
                case DataType.NDB:
                {
                    if (flags.HashAll)
                        HashNDB(buffer, flags);
                    else
                        ProcessNDB(buffer, flags);

                    break;
                }
                default:
                    Logger.Error("Nyotengu", $"Format for {flags.Path} is unknown!");
                    break;
            }
        }

        private static void ProcessOBJDB(Span<byte> buffer, NDB ndb, DatabaseFlags flags)
        {
            var db = new OBJDB(buffer, ndb);
            var filelist = Cethleann.ManagedFS.Nyotengu.LoadKTIDFileList(flags.FileList, flags.GameId);
            var filters = flags.TypeInfoFilter?.Split(',').Select(x => RDB.Hash(x.Trim())).ToHashSet() ?? new HashSet<uint>();
            foreach (var (ktid, (entry, properties)) in db.Entries)
            {
                if (filters.Count != 0 && !filters.Contains(entry.TypeInfoKTID)) continue;
                var lines = new List<string>
                {
                    $"KTID: {ktid:x8}",
                    $"KTID Name: {ktid.GetName(ndb, filelist) ?? "unnamed"}",
                    $"TypeInfo: {entry.TypeInfoKTID:x8}",
                    $"TypeInfo Name: {entry.TypeInfoKTID.GetName(ndb, filelist) ?? "unnamed"}"
                };

                foreach (var (property, values) in properties)
                {
                    lines.Add($"{property.PropertyKTID:x8} ({property.TypeId}): {string.Join(", ", values.Select(x => x?.ToString() ?? "null"))}");
                    if (property.TypeId == OBJDBPropertyType.KTID) lines.Add($"{property.PropertyKTID:x8} ({property.TypeId}) Names: {string.Join(", ", values.Select(x => (x is KTIDReference reference ? reference.GetName(ndb, filelist) : null) ?? "unnamed"))}");
                }

                foreach (var line in lines) Console.Out.WriteLine(line);

                Console.Out.WriteLine();
            }
        }

        private static void ProcessNDB(Span<byte> buffer, DatabaseFlags flags)
        {
            var name = new NDB(buffer);
            foreach (var (entry, strings) in name.Entries)
            {
                var filename = name.NameMap[entry.KTID];
                var text = $"{entry.KTID:x8},{RDB.Hash(strings[0]):x8},{strings.ElementAt(0)},{filename},{RDB.Hash(strings[1]):x8},{strings[1]}";
                if (strings.Length > 2) text += string.Join(string.Empty, strings.Skip(2));
                Console.WriteLine(text);
            }
        }

        private static void HashNDB(Span<byte> buffer, DatabaseFlags flags)
        {
            var name = new NDB(buffer);
            var hashes = new Dictionary<uint, string>();
            var typeInfo = new Dictionary<uint, string>();
            var extra = new Dictionary<uint, string>();
            foreach (var (_, strings) in name.Entries)
            {
                hashes[RDB.Hash(strings[0])] = strings[0];
                typeInfo[RDB.Hash(strings[1])] = strings[1];
                foreach (var str in strings.Skip(2)) extra[RDB.Hash(str)] = str;
            }

            foreach (var (hash, text) in hashes) Console.WriteLine($"{hash:x8}, {text}");

            foreach (var (hash, text) in typeInfo) Console.WriteLine($"{hash:x8}, {text}");

            foreach (var (hash, text) in extra) Console.WriteLine($"{hash:x8}, {text}");
        }
    }
}
