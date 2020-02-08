using System;
using System.Collections.Generic;
using Cethleann.Gust;
using Cethleann.Structure;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Manages PAK files
    /// </summary>
    [PublicAPI]
    public class Reisalin : IManagedFS
    {
        public Reisalin(DataGame gameid)
        {
            GameId = gameid;
        }

        /// <summary>
        ///     Lsof PAKs mounted
        /// </summary>
        public List<PAK> PAKs { get; set; } = new List<PAK>();

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int EntryCount { get; private set; }
        public DataGame GameId { get; }

        public Memory<byte> ReadEntry(int index)
        {
            foreach (var pak in PAKs)
            {
                if (index < pak.Entries.Count) return pak.ReadEntry(pak.Entries[index]);
                index -= pak.Entries.Count;
            }

            return Memory<byte>.Empty;
        }

        public Dictionary<string, string> LoadFileList(string filename = null, DataGame? game = null)
        {
            return null;
        }

        public string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            foreach (var pak in PAKs)
            {
                if (index < pak.Entries.Count) return pak.Entries[index].Filename;
                index -= pak.Entries.Count;
            }

            return null;
        }

        public void AddDataFS(string path) => AddDataFS(path, true);

        private void Dispose(bool disposing)
        {
            foreach (var pak in PAKs) pak.Dispose();
            if (!disposing) return;
            PAKs.Clear();
        }

        /// <summary>
        ///     Mounts a PAK
        /// </summary>
        /// <param name="path"></param>
        /// <param name="a18"></param>
        public void AddDataFS(string path, bool a18)
        {
            var pak = new PAK(path, a18);
            EntryCount += pak.Entries.Count;
            PAKs.Add(pak);
        }

        /// <summary>
        ///     Disposes
        /// </summary>
        ~Reisalin()
        {
            Dispose(false);
        }
    }
}
