using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Cethleann.Archive;
using Cethleann.ManagedFS.Options;
using Cethleann.ManagedFS.Options.Default;
using Cethleann.Structure;
using Cethleann.Structure.KTID;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     RDB Manager
    /// </summary>
    [PublicAPI]
    public class Nyotengu : IManagedFS
    {
        /// <summary>
        ///     Loads data
        /// </summary>
        /// <param name="options"></param>
        public Nyotengu(IManagedFSOptionsBase options)
        {
            if (options is IManagedFSOptions optionsLayer) GameId = optionsLayer.GameId;
            if (options is INyotenguOptions nyotenguOptions) Options = nyotenguOptions;
        }

        public INyotenguOptions Options { get; set; } = new NyotenguOptions();

        private Dictionary<uint, string> ExtList { get; set; }

        /// <summary>
        ///     List of RDBs loaded
        /// </summary>
        public List<RDB> RDBs { get; set; } = new List<RDB>();

        /// <summary>
        ///     Loaded FileList.csv
        /// </summary>
        public Dictionary<uint, string> FileList { get; set; } = new Dictionary<uint, string>();

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public int EntryCount { get; set; }

        /// <inheritdoc />
        public DataGame GameId { get; }

        /// <inheritdoc />
        public Memory<byte> ReadEntry(int index)
        {
            if (index < 0) return Memory<byte>.Empty;

            foreach (var rdb in RDBs)
            {
                if (index < rdb.Entries.Count) return rdb.ReadEntry(index);

                index -= rdb.Entries.Count;
            }

            return Memory<byte>.Empty;
        }

        /// <inheritdoc />
        public string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            return GetFilenameInternal(index);
        }

        /// <inheritdoc />
        public void AddDataFS(string path)
        {
            Logger.Success("Nyotengu", $"Loading {Path.GetFileName(path)}...");
            var rdb = new RDB(File.ReadAllBytes(path), Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path));
            EntryCount += rdb.Entries.Count;
            RDBs.Add(rdb);
        }

        /// <inheritdoc />
        public Dictionary<string, string> LoadFileList(string filename = null, DataGame? game = null)
        {
            FileList = LoadKTIDFileList(filename, game ?? GameId);
            return FileList.ToDictionary(x => x.Key.ToString(), y => y.Value);
        }

        public Memory<byte> ReadEntry(uint ktid)
        {
            foreach (var rdb in RDBs)
            {
                for (var i = 0; i < rdb.Entries.Count; i++)
                {
                    var (entry, _, _) = rdb.Entries[i];
                    if (entry.FileKTID == ktid) return ReadEntry(i);
                }
            }

            return Memory<byte>.Empty;
        }

        public static Dictionary<uint, string> LoadKTIDFileList(string filename = null, DataGame game = DataGame.None)
        {
            var loc = ManagedFSHelper.GetFileListLocation(filename, game, "rdb");
            var locShared = ManagedFSHelper.GetFileListLocation(filename, "RDBSHared", "rdb");
            var csv = ManagedFSHelper.GetFileList(locShared, 3).Concat(ManagedFSHelper.GetFileList(loc, 3)).ToArray();
            var fileList = new Dictionary<uint, string>();
            foreach (var (key, value) in csv.Select(x => (key: uint.Parse(x[1].ToLower(), NumberStyles.HexNumber), value: x[2])))
            {
                if (fileList.ContainsKey(key)) Logger.Warn("NYO", $"File List contains filename hash twice! ({key:x8}, {value}, {fileList[key]})");
                fileList[key] = value;
            }

            return fileList;
        }

        public static Dictionary<uint, (string, string)> LoadKTIDFileListEx(string filename = null, DataGame game = DataGame.None)
        {
            var loc = ManagedFSHelper.GetFileListLocation(filename, game, "rdb");
            var locShared = ManagedFSHelper.GetFileListLocation(filename, "RDBSHared", "rdb");
            var csv = ManagedFSHelper.GetFileList(locShared, 3).Concat(ManagedFSHelper.GetFileList(loc, 3)).ToArray();
            var fileList = new Dictionary<uint, (string, string)>();
            foreach (var (key, value) in csv.Select(x => (key: uint.Parse(x[1].ToLower(), NumberStyles.HexNumber), value: (x[0], x[2]))))
            {
                if (fileList.ContainsKey(key)) Logger.Warn("NYO", $"File List contains filename hash twice! ({key}, {value}, {fileList[key]})");
                fileList[key] = value;
            }

            return fileList;
        }

        private string GetFilenameInternal(int index)
        {
            var prefix = "";
            var entry = default(RDBEntry);
            var selectedRdb = default(RDB);
            foreach (var rdb in RDBs)
            {
                if (index >= rdb.Entries.Count)
                {
                    index -= rdb.Entries.Count;
                    continue;
                }

                prefix = rdb.Name;
                entry = rdb.GetEntry(index);
                selectedRdb = rdb;
                break;
            }

            if (selectedRdb == null) return null;

            if (!selectedRdb.NameDatabase.ExtMap.TryGetValue(entry.TypeInfoKTID, out var ext) && (!ExtList.TryGetValue(entry.TypeInfoKTID, out ext) || string.IsNullOrEmpty(ext))) ext = selectedRdb.NameDatabase.HashMap.TryGetValue(entry.TypeInfoKTID, out ext) ? ext.Split(':').Last() : entry.TypeInfoKTID.ToString("x8");

            prefix += $@"\{ext}";

            if ((!selectedRdb.NameDatabase.NameMap.TryGetValue(entry.FileKTID, out var path) || string.IsNullOrWhiteSpace(path)) && (!FileList.TryGetValue(entry.FileKTID, out path) || string.IsNullOrWhiteSpace(path)))
            {
                path = $"{entry.FileKTID:x8}.{ext}";
            }
            else
            {
                if (string.IsNullOrEmpty(Path.GetExtension(path))) path += $".{ext}";
                if (Options.NyotenguPrefixFilenames) path = Path.Combine(Path.GetDirectoryName(path), $"{entry.FileKTID:x8}{RDB.HASH_PREFIX_STR}{Path.GetFileNameWithoutExtension(path)}{RDB.HASH_SUFFIX_STR}.{Path.GetExtension(path)}");
            }

            return $@"{prefix}\{path}";
        }

        /// <summary>
        ///     Load typeid extension list
        /// </summary>
        /// <param name="filename"></param>
        public void LoadExtList(string filename = null)
        {
            ExtList = ManagedFSHelper.GetSimpleFileList(ManagedFSHelper.GetFileListLocation(filename, "RDBExt", "rdb"), DataGame.None, "rdb").ToDictionary(x => uint.Parse(x.Key, NumberStyles.HexNumber), y => y.Value);
        }

        /// <summary>
        ///     Disposes
        /// </summary>
        ~Nyotengu()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            foreach (var rdb in RDBs) rdb.Dispose();
            if (!disposing) return;
            RDBs.Clear();
            EntryCount = 0;
        }

        public void SaveGeneratedFileList(string filename = null, DataGame? game = null)
        {
            var filelist = LoadKTIDFileListEx(filename, game ?? GameId);
            foreach (var rdb in RDBs)
            foreach (var (hash, name) in rdb.NameDatabase.NameMap)
                filelist[hash] = (rdb.Name, name);

            File.WriteAllText(ManagedFSHelper.GetFileListLocation(filename, game ?? GameId, "rdb-generated"), string.Join("\n", filelist.OrderBy(x => $"{x.Value.Item1}{x.Key:x8}").Select(x => $"{x.Value.Item1},{x.Key:x8},{x.Value.Item2}")));
        }
    }
}
