using Cethleann.Archive;
using Cethleann.ManagedFS.Options;
using Cethleann.ManagedFS.Options.Default;
using Cethleann.Structure;
using Cethleann.Structure.KTID;
using DragonLib.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Cethleann.ManagedFS
{
    public class Zhao : IManagedFS
    {
        /// <summary>
        ///     Loads data
        /// </summary>
        /// <param name="options"></param>
        public Zhao(IManagedFSOptionsBase options)
        {
            if (options is IManagedFSOptions optionsLayer) GameId = optionsLayer.GameId;
            if (options is INyotenguOptions nyotenguOptions) Options = nyotenguOptions;
        }

        /// <summary>
        ///     Nyotengu specific Options
        /// </summary>
        public INyotenguOptions Options { get; set; } = new NyotenguOptions();

        public List<PRDB> RDBs { get; } = new List<PRDB>();

        public Dictionary<KTIDReference, string> FileList { get; set; } = new Dictionary<KTIDReference, string>();

        public Dictionary<KTIDReference, string> ExtList { get; set; } = new Dictionary<KTIDReference, string>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int EntryCount { get; private set; }
        public string GameId { get; } = "";

        public Memory<byte> ReadEntry(int index)
        {
            foreach (var rdb in RDBs)
            {
                if (index >= rdb.Entries.Count)
                {
                    index -= rdb.Entries.Count;
                    continue;
                }

                var (_, p) = rdb.Entries[index];
                return new Memory<byte>(rdb.ReadEntry(p).ToArray());
            }

            return Memory<byte>.Empty;
        }

        public Dictionary<string, string> LoadFileList(string? filename = null, string? game = null)
        {
            FileList = Nyotengu.LoadKTIDFileListShared(filename, game ?? GameId);
            return FileList.ToDictionary(x => x.Key.ToString(CultureInfo.InvariantCulture), y => y.Value);
        }

        public string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            var entry = default(PRDBEntry);
            foreach (var rdb in RDBs)
            {
                if (index >= rdb.Entries.Count)
                {
                    index -= rdb.Entries.Count;
                    continue;
                }

                (entry, _) = rdb.Entries[index];
                break;
            }

            if (!ExtList.TryGetValue(entry.TypeInfoKTID, out var actualExt)) actualExt = $"{entry.TypeInfoKTID:x8}";

            if (!FileList.TryGetValue(entry.FileKTID, out var name))
            {
                name = $"{entry.FileKTID:x8}.{actualExt}";
            }
            else
            {
                if (string.IsNullOrEmpty(Path.GetExtension(name))) name += $".{actualExt}";
                if (Options.NyotenguPrefixFilenames) name = Path.Combine(Path.GetDirectoryName(name) ?? string.Empty, $"{entry.FileKTID:x8}{RDB.HASH_PREFIX_STR}{Path.GetFileNameWithoutExtension(name)}{RDB.HASH_SUFFIX_STR}.{Path.GetExtension(name)}");
            }

            return $"{actualExt}/{name}";
        }

        public void AddDataFS(string path)
        {
            Logger.Success("Zhao", $"Loading {Path.GetFileName(path)}...");
            var rdb = new PRDB(File.OpenRead(path));
            EntryCount += rdb.Entries.Count;
            RDBs.Add(rdb);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            foreach (var prdb in RDBs) prdb.Dispose();
            RDBs.Clear();
            EntryCount = 0;
        }

        public void LoadExtList(string? filename = null) => ExtList = ManagedFSHelper.GetSimpleFileList(ManagedFSHelper.GetFileListLocation(filename, "RDBExt", "rdb"), "", "rdb").ToDictionary(x => KTIDReference.Parse(x.Key, NumberStyles.HexNumber), y => y.Value);
    }
}
