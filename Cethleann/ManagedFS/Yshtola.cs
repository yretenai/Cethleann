using System;
using System.Collections.Generic;
using System.IO;
using Cethleann.Archive;
using Cethleann.ManagedFS.Options;
using Cethleann.ManagedFS.Support;
using Cethleann.Structure;
using Cethleann.Structure.Archive;
using DragonLib.IO;
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
        /// <param name="options"></param>
        /// <param name="settings"></param>
        public Yshtola(IManagedFSOptionsBase options, YshtolaSettings settings)
        {
            Settings = settings;
            if (options is IManagedFSOptions optionsLayer) GameId = optionsLayer.GameId;
        }

        /// <summary>
        ///     File List for hashes
        /// </summary>
        public Dictionary<string, string> FileList { get; set; } = new Dictionary<string, string>();

        /// <summary>
        ///     Settings to use for decryption and loading.
        /// </summary>
        public YshtolaSettings Settings { get; }

        /// <summary>
        ///     ID Table
        /// </summary>
        public List<PKGTBL> Tables { get; set; } = new List<PKGTBL>();

        /// <summary>
        ///     Root directory, the one that contains COMMON.
        /// </summary>
        public string[]? Root { get; set; }

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public int EntryCount { get; private set; }

        /// <inheritdoc />
        public DataGame GameId { get; }

        /// <inheritdoc />
        public Memory<byte> ReadEntry(int index)
        {
            foreach (var table in Tables)
            {
                if (index < table.Entries.Length)
                {
                    foreach (var root in Root ?? Array.Empty<string>())
                    {
                        var filepath = Path.Combine(root, table.Entries[index].Path(table.Buffer, table.Header.Offset) ?? string.Empty);
                        if (File.Exists(filepath)) return new Memory<byte>(table.Read(File.ReadAllBytes(filepath), GameId, table.Entries[index], Settings.XorTruth, Settings.Multiplier, Settings.Divisor).ToArray());
                    }

                    return Memory<byte>.Empty;
                }

                index -= table.Entries.Length;
            }

            return Memory<byte>.Empty;
        }

        /// <inheritdoc />
        public Dictionary<string, string> LoadFileList(string? filename = null, DataGame? game = null)
        {
            FileList = ManagedFSHelper.GetSimpleFileList(filename, game ?? GameId, "pkginfo");
            return FileList;
        }

        /// <inheritdoc />
        public string? GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            foreach (var table in Tables)
            {
                if (index < table.Entries.Length)
                {
                    var entry = table.Entries[index];
                    if (entry.OriginalPathOffset > -1)
                        return entry.OriginalPath(table.Buffer, table.Header.Offset);

                    var path = entry.Path(table.Buffer, table.Header.Offset) ?? string.Empty;
                    if (!FileList.TryGetValue(path, out var resultPath)) resultPath = path + $".{ext}";
                    return resultPath;
                }

                index -= table.Entries.Length;
            }

            throw new ArgumentOutOfRangeException();
        }

        /// <inheritdoc />
        public void AddDataFS(string path)
        {
            if (Root == null) return;
            foreach (var root in Root)
            {
                var tablePath = Path.Combine(root, path);
                if (!File.Exists(tablePath))
                {
                    Logger.Error("Yshtola", $"{path} doesn't exist!");
                    continue;
                }

                Logger.Success("Yshtola", $"Loading {path}...");
                var table = new PKGTBL(File.ReadAllBytes(tablePath), GameId, IDTableFlags.Compressed | IDTableFlags.Encrypted, Settings.XorTruth, Settings.Multiplier, Settings.Divisor);
                Tables.Add(table);
                EntryCount += table.Entries.Length;
                break;
            }
        }
    }
}
