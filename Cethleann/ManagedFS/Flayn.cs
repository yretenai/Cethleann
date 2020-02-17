using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var localId = index;
            for (var i = 0; i < Data.Count + (PatchEntryCount > 0 ? 1 : 0); ++i)
            {
                if (PatchEntryCount > 0)
                {
                    var (info0, info1) = Patch;
                    if (i == 1)
                    {
                        if (localId < info1.Entries.Count) return info1.ReadEntry(PatchRomFS, localId);

                        localId -= info1.Entries.Count;
                        continue;
                    }

                    var buffer = info0.ReadEntry(PatchRomFS, localId);
                    if (buffer.Length > 0) return buffer;
                }

                var actualIndex = i;
                if (i > 0 && PatchEntryCount > 0) actualIndex -= 1;
                var (data, stream, _, _) = Data[actualIndex];
                var dataCount = data.Entries.Count;
                if (i == 0) dataCount += 1;
                if (localId < dataCount)
                {
                    var buffer = data.ReadEntry(stream, localId);
                    if (GameId == DataGame.ThreeHouses && i > 0 && buffer.Length == 0) continue;
                    return buffer;
                }
                localId -= dataCount;
            }

            return Memory<byte>.Empty;
        }

        /// <inheritdoc />
        public Dictionary<string, string> LoadFileList(string filename = null, DataGame? game = null)
        {
            FileList = ManagedFSHelper.GetNamedFileList(filename, game ?? GameId, "link");
            return FileList;
        }

        /// <inheritdoc />
        public string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            if (dataType == DataType.Compressed || dataType == DataType.CompressedChonky) ext = ext == "gz" ? "bin.gz" : ext + ".gz";

            var id = "0";
            var generatedPrefix = index.ToString();
            var prefix = "";
            var localId = index;
            for (var i = 0; i < Data.Count + (PatchEntryCount > 0 ? 1 : 0); ++i)
            {
                if (localId < 0) break;
                if (PatchEntryCount > 0 && i == 1)
                {
                    var (_, info1) = Patch;
                    if (localId > info1.Entries.Count)
                    {
                        localId -= info1.Entries.Count;
                        continue;
                    }

                    generatedPrefix = "PATCH - ";
                    id = $"PATCH_{localId}";
                    prefix = "patch/";
                    break;
                }

                var actualIndex = i;
                if (i > 0 && PatchEntryCount > 0) actualIndex -= 1;
                var (data, _, _, linkname) = Data[actualIndex];
                var dataCount = data.Entries.Count;
                if (i == 0) dataCount += 1;
                if (localId > dataCount)
                {
                    localId -= dataCount;
                    continue;
                }

                generatedPrefix = GameId switch
                {
                    DataGame.ThreeHouses => i == 0 ? "" : "DLC - ",
                    _ => ""
                };
                id = GameId switch
                {
                    DataGame.ThreeHouses => $"{(i == 0 ? "" : "DLC_")}{localId}",
                    _ => $"{linkname}_{localId}"
                };
                prefix = GameId switch
                {
                    DataGame.ThreeHouses => $"{(i == 0 ? "" : "dlc")}/",
                    _ => $"{linkname}"
                };
                break;
            }


            if (!FileList.TryGetValue(id, out var path)) path = (ext == "bin" || ext == "bin.gz" ? $"misc/unknown/{generatedPrefix}{localId}.{ext}" : $"misc/formats/{ext.ToUpper().Replace('.', '_')}/{generatedPrefix}{localId}.{ext}");
            else path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + $".{ext}");
            if (ext.EndsWith(".gz") && !path.EndsWith(".gz")) path += ".gz";
            return $@"{prefix}\{path}";
        }

        /// <summary>
        ///     Adds a DATA container, usually base games
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="FileNotFoundException"></exception>
        // TODO: Separate Main and DLC.
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
