using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Cethleann.Koei;
using Cethleann.Structure;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Management class for DATA0 and INFO0 files.
    /// </summary>
    [PublicAPI]
    public class Flayn : IManagedFS
    {
        /// <summary>
        ///     Loads data
        /// </summary>
        /// <param name="game"></param>
        public Flayn(DataGame game = DataGame.None)
        {
            GameId = game;
        }
        
        /// <summary>
        ///     Game data
        /// </summary>
        public List<(DATA0 DATA0, Stream DATA1, string romfs, string name)> Data { get; private set; } = new List<(DATA0, Stream, string, string)>();

        /// <summary>
        ///     Patch data
        /// </summary>
        public (INFO0 INFO0, INFO1 INFO1) Patch { get; private set; }

        /// <summary>
        ///     LINKIDX/LINKDATA patterns
        /// </summary>
        public List<(string idxPattern, string dataPattern, char[] separators, string type)> Patterns { get; set; } = new List<(string idxPattern, string dataPattern, char[] separators, string type)>();

        /// <summary>
        ///     Root directory of the Patch rom:/
        /// </summary>
        public string PatchRomFS { get; private set; }

        /// <summary>
        ///     Maximum number of entries found in the base container
        /// </summary>
        public int RootEntryCount { get; private set; }

        /// <summary>
        ///     Maximum number of entries found in the DLC containers
        /// </summary>
        public int DataEntryCount { get; private set; }

        /// <summary>
        ///     Maximum number of entries found in the latest patch container.
        /// </summary>
        public int PatchEntryCount { get; private set; }

        /// <summary>
        ///     LINKDATA Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Prefix LINKDATA archive name to files
        /// </summary>
        public bool PrefixLinkData { get; set; }

        /// <summary>
        ///     Loaded FileList.csv
        /// </summary>
        public Dictionary<string, string> FileList { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public DataGame GameId { get; private set; }

        /// <summary>
        ///     Maximum number of entries found in both containers and patches
        /// </summary>
        public int EntryCount => RootEntryCount + PatchEntryCount + DataEntryCount;

        /// <summary>
        ///     Cleans managed data
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public Memory<byte> ReadEntry(int index)
        {
            if (index >= EntryCount) throw new IndexOutOfRangeException($"Index {index} does not exist!");

            if (index >= RootEntryCount)
            {
                if (index < PatchEntryCount + RootEntryCount) return FindPatch(index - RootEntryCount);
            }

            try
            {
                var (info0, _) = Patch;
                if (info0 != null)
                {
                    var entry = info0.ReadEntry(PatchRomFS, index);
                    if (entry.Length > 0) return entry;
                }
            }
            catch (IndexOutOfRangeException)
            {
                // ignored.
            }

            for (var i = 0; i < Data.Count; i++)
            {
                if (index < 0) break;
                
                var (data, stream, _, _) = Data[i];
                if (index >= data.Entries.Count)
                {
                    index -= data.Entries.Count;
                    if (i == 0) index -= PatchEntryCount;
                    continue;
                }

                try
                {
                    var blob = data.ReadEntry(stream, index);
                    if (blob.Length == 0) continue;
                    return blob;
                }
                catch (IndexOutOfRangeException)
                {
                    // ignored.
                }
            }

            return Memory<byte>.Empty;
        }

        /// <inheritdoc />
        public Dictionary<string, string> LoadFileList(string filename = null, DataGame? game = null)
        {
            FileList = ManagedFSHelper.GetSimpleFileList(filename, game ?? GameId, "link");
            return FileList;
        }

        /// <inheritdoc />
        public string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            if (dataType == DataType.Compressed || dataType == DataType.CompressedChonky) ext += ".gz";

            var path = default(string);
            var prefix = "";
            var logicalId = default(string);
            if (index >= RootEntryCount)
            {
                if (index < RootEntryCount + PatchEntryCount)
                {
                    if (GameId == DataGame.ThreeHouses)
                    {
                        prefix = "dlc/";
                        logicalId = $"DLC_{index - RootEntryCount - PatchEntryCount} - {index}";
                    }
                }
                else
                {
                    prefix = "patch/";
                    logicalId = $"PATCH_{index - RootEntryCount} - {index}";
                    var info1 = Patch.INFO1;
                    path = info1.GetPath(index - RootEntryCount);
                    if (!string.IsNullOrWhiteSpace(path) && path != "nx/")
                    {
                        if (path.StartsWith("nx/", StringComparison.InvariantCultureIgnoreCase)) path = path.Substring(3);
                        var dir = Path.GetDirectoryName(path);
                        var file = Path.GetFileName(path);
                        path = Path.Combine(dir, $"{logicalId} - {file}");
                    }
                }
            }

            if (PrefixLinkData) logicalId = $"{Name}_{(prefix.Length > 0 ? prefix : index.ToString())}";

            var temp = path;
            if (!FileList.TryGetValue(logicalId ?? index.ToString(), out path)) path = temp ?? (ext == "bin" || ext == "bin.gz" ? $"misc/unknown/{logicalId ?? index.ToString()}.{ext}" : $"misc/formats/{ext.ToUpper().Replace('.', '_')}/{logicalId ?? index.ToString()}.{ext}");
            else path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + $".{ext}");
            if (ext.EndsWith(".gz") && !path.EndsWith(".gz")) path += ".gz";
            return prefix + path;
        }

        /// <summary>
        ///     Adds a DATA container, usually base games
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void AddDataFS(string path)
        {
            foreach (var (idxPattern, dataPattern, separators, type) in Patterns)
            {
                foreach (var file in Directory.GetFiles(path, idxPattern, SearchOption.TopDirectoryOnly))
                {
                    var binPath = dataPattern;
                    if (separators.Length > 0)
                    {
                        var chunks = Path.GetFileName(file).Split(separators).Select(x => (object) x).ToArray();
                        binPath = Path.Combine(path, string.Format(dataPattern, chunks));
                    }

                    if (File.Exists(binPath))
                    {
                        switch (type)
                        {
                            case "DATA":
                                AddDataFSInternal(file, binPath);
                                break;
                            case "INFO":
                                AddInfoFSInternal(file, binPath);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Cleanup
        /// </summary>
        ~Flayn()
        {
            Dispose(false);
        }

        private void AddDataFSInternal(string idxPath, string binPath)
        {
            GC.ReRegisterForFinalize(this);

            var fullPath = Path.GetFullPath(Path.GetDirectoryName(idxPath));
            if (Data.Any(x => x.romfs == fullPath)) return;
            var set = (new DATA0(idxPath), File.OpenRead(binPath), fullPath, Path.GetFileNameWithoutExtension(idxPath));
            Data.Add(set);
            if (Data.Count == 1)
            {
                RootEntryCount = set.Item1.Entries.Count + 1; // thanks Koei.
            }
            else
            {
                DataEntryCount = Data.Skip(1).Max(x => x.DATA0.Entries.Count);
            }
        }

        /// <summary>
        ///     Adds a patch container.
        /// </summary>
        /// <param name="info0Path"></param>
        /// <param name="info1Path"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void AddInfoFSInternal(string info0Path, string info1Path)
        {
            PatchRomFS = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(info0Path), ".."));
            Patch = (new INFO0(info0Path), new INFO1(info1Path));
            PatchEntryCount = Patch.INFO1?.Entries.Count ?? 0;
        }

        private Memory<byte> FindPatch(int index)
        {
            try
            {
                var (_, info1) = Patch;
                var entry = info1.ReadEntry(PatchRomFS, index);
                if (entry.Length > 0) return entry;
            }
            catch (IndexOutOfRangeException)
            {
                // ignored.
            }

            return Memory<byte>.Empty;
        }

        private void Dispose(bool disposing)
        {
            foreach (var (_, stream, _, _) in Data) stream.Dispose();
            if (!disposing) return;
            Data = new List<(DATA0, Stream, string, string)>();
            DataEntryCount = 0;
            PatchEntryCount = 0;
            RootEntryCount = 0;
        }

        public void LoadPatterns(string filename = null)
        {
            Patterns = ManagedFSHelper.GetFileList(ManagedFSHelper.GetFileListLocation(filename, "LINKDATAPatterns", "link"), 4).Select(x => (x[0], x[2], x[1].ToCharArray(), x[3])).ToList();
        }
    }
}
