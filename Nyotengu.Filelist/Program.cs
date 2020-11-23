using System.IO;
using System.Linq;
using Cethleann.Archive;
using Cethleann.KTID;
using DragonLib.CLI;
using DragonLib.IO;

namespace Nyotengu.Filelist
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Logger.PrintVersion("Nyotengu");
            var flags = CommandLineFlags.ParseFlags<FilelistFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null || string.IsNullOrWhiteSpace(flags.GameDir)) return;

            Logger.Info("Nyotengu", "Generating filename list...");
            var nyotengu = new Cethleann.ManagedFS.Nyotengu(flags);
            nyotengu.LoadFileList(flags.FileList, flags.GameId);
            foreach (var rdb in Directory.GetFiles(flags.GameDir, "*.rdb")) nyotengu.AddDataFS(rdb);
            nyotengu.SaveGeneratedFileList(null, flags.GameId);
            
            Logger.Info("Nyotengu", "Generating KTID property list...");
            var propertyList = Cethleann.ManagedFS.Nyotengu.LoadKTIDFileListEx(null,  "PropertyList");
            foreach (var rdb in nyotengu.RDBs)
            {
                for (var i = 0; i < rdb.Entries.Count; ++i)
                {
                    var entry = rdb.GetEntry(i);
                    if (entry.FileKTID == rdb.Header.NameDatabaseKTID) continue;
                    if (entry.TypeInfoKTID != 0xbf6b52c7) continue;
                    var namedb = new NDB(rdb.ReadEntry(i).Span);
                    foreach (var (_, strings) in namedb.Entries)
                    {
                        if (strings[1].Length == 0) continue;
                        propertyList[RDB.Hash(strings[1])] = ("TypeInfo", strings[1]);
                        foreach (var str in strings.Skip(2))
                        {

                            propertyList[RDB.Hash(str)] = ("Property", str);
                        }
                    }
                }
            }
            Cethleann.ManagedFS.Nyotengu.SaveGeneratedFileList(propertyList, null, "DEBUG");
        }
    }
}
