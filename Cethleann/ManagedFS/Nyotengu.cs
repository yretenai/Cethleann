using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Cethleann.Archive;
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
        /// <param name="game"></param>
        public Nyotengu(DataGame game = DataGame.None)
        {
            GameId = game;
        }

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
            var rdb = new RDB(File.ReadAllBytes(path), Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path));
            EntryCount += rdb.Entries.Count;
            RDBs.Add(rdb);
        }

        /// <inheritdoc />
        public Dictionary<string, string> LoadFileList(string filename = null, DataGame? game = null)
        {
            var loc = ManagedFSHelper.GetFileListLocation(filename, game ?? GameId, "rdb");
            var locShared = ManagedFSHelper.GetFileListLocation(filename, "RDBSHared", "rdb");
            var csv = ManagedFSHelper.GetFileList(locShared, 3).Concat(ManagedFSHelper.GetFileList(loc, 3)).ToArray();
            FileList = new Dictionary<uint, string>();
            foreach (var (key, value) in csv.Select(x => (key: uint.Parse(x[1].ToLower(), NumberStyles.HexNumber), value: x[2])))
            {
                if (FileList.ContainsKey(key)) Logger.Warn("NYO", $"File List contains filename hash twice! ({key}, {value}, {FileList[key]})");
                FileList[key] = value;
            }

            return FileList.ToDictionary(x => x.Key.ToString(), y => y.Value);
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

            if (!selectedRdb.NameDatabase.ExtMap.TryGetValue(entry.TypeInfoKTID, out var ext) && (!ExtList.TryGetValue(entry.TypeInfoKTID, out ext) || string.IsNullOrEmpty(ext)) && !selectedRdb.NameDatabase.ExtMapRaw.TryGetValue(entry.TypeInfoKTID, out ext)) ext = entry.TypeInfoKTID.ToString("x8");

            prefix += $@"\{ext}";

            if (selectedRdb.NameDatabase.NameMap.TryGetValue(entry.FileKTID, out var path)) path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + $".{ext}");
            else if (FileList.TryGetValue(entry.FileKTID, out path)) path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + $".{ext}");
            else path = $"{entry.FileKTID:x8}.{ext}";
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
    }
}
