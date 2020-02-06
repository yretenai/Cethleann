using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cethleann.Koei;
using Cethleann.Structure;
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

        private Dictionary<string, string> ExtList { get; set; }

        /// <summary>
        ///     List of RDBs loaded
        /// </summary>
        public List<RDB> RDBs { get; set; } = new List<RDB>();

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
        public Dictionary<string, string> FileList { get; set; }

        /// <inheritdoc />
        public string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            var prefix = "RDBArchive";
            var entry = default(RDBEntry);
            foreach (var rdb in RDBs)
            {
                if (index >= rdb.Entries.Count)
                {
                    index -= rdb.Entries.Count;
                    continue;
                }

                prefix = rdb.Name;
                entry = rdb.GetEntry(index);
                break;
            }

            if (ext == "bin")
                if (!ExtList.TryGetValue(entry.TypeId.ToString("x8"), out ext))
                    ext = entry.TypeId.ToString("x8");

            prefix += $@"\{ext}";

            if (!FileList.TryGetValue($"{prefix}_{entry.FileId:x8}", out var path)) path = $"{entry.FileId:x8}.{ext}";
            else path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + $".{ext}");
            return $@"{prefix}\{path}";
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
            var loc = ManagedFSHelpers.GetFileListLocation(filename, game ?? GameId);
            var csv = ManagedFSHelpers.GetFileList(loc, 3);
            FileList = csv.ToDictionary(x => $"{x[0]}_{x[1]}", y => y[2]);
            ExtList = ManagedFSHelpers.GetSimpleFileList("filelist-RDB.csv", DataGame.None);
            return FileList;
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
