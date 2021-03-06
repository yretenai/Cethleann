using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cethleann.Archive;
using Cethleann.ManagedFS.Options;
using Cethleann.Structure;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Management class for DATA0 and INFO0 files.
    /// </summary>
    [PublicAPI]
    public class Leonhart : IManagedFS
    {
        /// <summary>
        ///     Loads data
        /// </summary>
        /// <param name="options"></param>
        public Leonhart(IManagedFSOptionsBase options)
        {
            if (options is IManagedFSOptions optionsLayer) GameId = optionsLayer.GameId;
        }

        /// <summary>
        ///     Game data
        /// </summary>
        public List<(LINKDATA linkdata, string name)> Data { get; private set; } = new List<(LINKDATA, string name)>();

        /// <summary>
        ///     Loaded FileList.csv
        /// </summary>
        public Dictionary<string, string> FileList { get; set; } = new Dictionary<string, string>();

        /// <summary>
        ///     Game ID of the game.
        /// </summary>
        public string GameId { get; private set; } = "";


        /// <inheritdoc />
        public int EntryCount { get; private set; }

        /// <summary>
        ///     Cleans managed data
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Reads an entry from the first valid (non-zero) storage.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Memory<byte> ReadEntry(int index)
        {
            if (index >= EntryCount) throw new IndexOutOfRangeException($"Index {index} does not exist!");
            foreach (var (linkdata, _) in Data)
            {
                if (index < linkdata.Entries.Length) return linkdata.ReadEntry(linkdata.Entries[index]);
                index -= linkdata.Entries.Length;
            }

            return Memory<byte>.Empty;
        }

        /// <inheritdoc />
        public Dictionary<string, string> LoadFileList(string? filename = null, string? game = null)
        {
            FileList = ManagedFSHelper.GetNamedFileList(filename, game ?? GameId, "link");
            return FileList;
        }

        /// <summary>
        ///     Attempts to get a valid filename/filepath.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ext"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            var prefix = "LINKDATA_UNKNOWN";
            foreach (var (linkdata, name) in Data)
            {
                if (index >= linkdata.Entries.Length)
                {
                    index -= linkdata.Entries.Length;
                    continue;
                }

                prefix = name;
                break;
            }

            if (!FileList.TryGetValue($"{prefix}_{index}", out var path)) path = ext == "bin" || ext == "bin.gz" ? $"misc/unknown/{index.ToString()}.{ext}" : $"misc/formats/{ext.ToUpper().Replace('.', '_')}/{index.ToString()}.{ext}";
            else path = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, Path.GetFileNameWithoutExtension(path) + $".{ext}");
            if (ext.EndsWith(".gz") && !path.EndsWith(".gz")) path += ".gz";
            return $@"{prefix}\{path}";
        }

        /// <summary>
        ///     Adds a LINKDATA container, usually base games
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void AddDataFS(string path)
        {
            GC.ReRegisterForFinalize(this);

            var files = Directory.GetFiles(path, "LINKDATA_*", SearchOption.AllDirectories).ToList();

            foreach (var file in files)
            {
                Logger.Success("Leonhart", $"Loading {Path.GetFileName(file)}...");
                var linkdata = new LINKDATA(File.OpenRead(file));
                Data.Add((linkdata, Path.GetFileNameWithoutExtension(file)));
                EntryCount += linkdata.Entries.Length;
            }
        }

        /// <summary>
        ///     Cleanup
        /// </summary>
        ~Leonhart() => Dispose(false);

        private void Dispose(bool disposing)
        {
            foreach (var (linkdata, _) in Data) linkdata.Dispose();
            if (!disposing) return;
            Data = new List<(LINKDATA, string)>();
            EntryCount = 0;
        }
    }
}
