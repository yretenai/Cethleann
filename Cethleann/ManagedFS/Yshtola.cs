using System;
using System.Collections.Generic;
using System.IO;
using Cethleann.ManagedFS.Support;
using Cethleann.Ninja;
using Cethleann.Structure;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Manages TN files
    /// </summary>
    [PublicAPI]
    public class Yshtola : IManagedFS
    {
        /// <summary>
        ///     Initialize with standard data.
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="root"></param>
        /// <param name="settings"></param>
        public Yshtola(DataGame gameId, string root, YshtolaSettings settings)
        {
            GameId = gameId;
            Settings = settings;
            Root = root;

            foreach (var tableName in settings.TableNames) AddDataFS(tableName);
        }

        /// <summary>
        ///     Settings to use for decryption and loading.
        /// </summary>
        public YshtolaSettings Settings { get; }

        /// <summary>
        ///     ID Table
        /// </summary>
        public List<IDTable> Tables { get; set; } = new List<IDTable>();

        /// <summary>
        ///     Root directory, the one that contains COMMON.
        /// </summary>
        public string Root { get; set; }

        public void Dispose() { }

        public int EntryCount { get; }
        public DataGame GameId { get; }
        public Dictionary<string, string> FileList { get; set; }

        public Memory<byte> ReadEntry(int index)
        {
            foreach (var table in Tables)
            {
                if (index < table.Entries.Length) return new Memory<byte>(table.Read(File.ReadAllBytes(Path.Combine(Root, table.Entries[index].Path(table.Buffer, table.Header.Offset))), table.Entries[index], Settings.XorTruth, Settings.Multiplier, Settings.Divisor).ToArray());
                index -= table.Entries.Length;
            }

            return Memory<byte>.Empty;
        }

        public Dictionary<string, string> LoadFileList(string filename = null, DataGame? game = null) => null;

        public string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            foreach (var table in Tables)
            {
                if (index < table.Entries.Length) return table.Entries[index].Path(table.Buffer, table.Header.Offset);
                index -= table.Entries.Length;
            }

            throw new ArgumentOutOfRangeException();
        }

        public void AddDataFS(string path)
        {
            var tablePath = Path.Combine(Root, path);
            if (!File.Exists(tablePath)) return;
            Tables.Add(new IDTable(File.ReadAllBytes(tablePath), IDTableFlags.Compressed | IDTableFlags.Encrypted, Settings.XorTruth, Settings.Multiplier, Settings.Divisor));
        }
    }
}
