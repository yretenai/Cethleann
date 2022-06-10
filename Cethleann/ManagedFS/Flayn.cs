using Cethleann.Archive;
using Cethleann.ManagedFS.Options;
using Cethleann.ManagedFS.Options.Default;
using Cethleann.Structure;
using DragonLib.IO;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScrambleLinkEncryption = Cethleann.Compression.Scramble.LinkEncryption;

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
        /// <param name="options"></param>
        public Flayn(IManagedFSOptionsBase options)
        {
            if (options is IManagedFSOptions optionsLayer) GameId = optionsLayer.GameId;
            if (options is IFlaynOptions flaynOptions) Options = flaynOptions;
        }

        /// <summary>
        ///     Nyotengu specific Options
        /// </summary>
        public IFlaynOptions Options { get; set; } = new FlaynOptions();

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
        public string? PatchRomFS { get; private set; }

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
        ///     Loaded FileList.csv
        /// </summary>
        public Dictionary<string, string> FileList { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public string GameId { get; private set; } = "";

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
                if (PatchEntryCount > 0 && PatchRomFS != null)
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
                    if (GameId == "ThreeHouses" && i > 0 && buffer.Length == 0) continue;

                    // for Scramble, all non-compressed entries are encrypted
                    // conversly, if an entry is compressed, it can't also be encrypted
                    if (GameId == "Scramble" && localId < data.Entries.Count && !data.Entries[localId].IsCompressed)
                        ScrambleLinkEncryption.Decrypt(buffer.Span, (uint) localId);
                    return buffer;
                }

                localId -= dataCount;
            }

            return Memory<byte>.Empty;
        }

        /// <inheritdoc />
        public Dictionary<string, string> LoadFileList(string? filename = null, string? game = null)
        {
            FileList = ManagedFSHelper.GetNamedFileList(filename, game ?? GameId, "link");
            return FileList;
        }

        /// <inheritdoc />
        public string GetFilename(int index, string? ext = "bin", DataType dataType = DataType.None)
        {
            if (dataType == DataType.Compressed || dataType == DataType.CompressedChonky) ext = ext == "gz" ? "bin.gz" : ext + ".gz";

            ext ??= "bin";

            var id = "0";
            var generatedPrefix = index.ToString();
            var prefix = string.Empty;
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
                    "ThreeHouses" => i == 0 ? string.Empty : "DLC - ",
                    _ => string.Empty
                };
                id = GameId switch
                {
                    "ThreeHouses" => $"{(i == 0 ? string.Empty : "DLC_")}{localId}",
                    "Scramble" => $"{localId}",
                    _ => $"{linkname}_{localId}"
                };
                prefix = GameId switch
                {
                    "ThreeHouses" => $"{(i == 0 ? string.Empty : "dlc")}/",
                    _ => $"{linkname}"
                };
                break;
            }

            if (!FileList.TryGetValue(id, out var path))
            {
                path = ext == "bin" || ext == "bin.gz" ? $"misc/unknown/{generatedPrefix}{localId}.{ext}" : $"misc/formats/{ext.ToUpper().Replace('.', '_')}/{generatedPrefix}{localId}.{ext}";
            }
            else
            {
                ext = GameId switch
                {
                    "Scramble" => Path.GetExtension(path) ?? ext,
                    _ => $".{ext}"
                };
                path = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, Path.GetFileNameWithoutExtension(path) + $"{ext}");
            }
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

        /// <summary>
        ///     Cleanup
        /// </summary>
        ~Flayn() => Dispose(false);

        private void AddDataFSInternal(string idxPath, string binPath)
        {
            GC.ReRegisterForFinalize(this);

            Logger.Success("Flayn", $"Loading {Path.GetFileName(idxPath)}...");

            var fullPath = Path.GetFullPath(Path.GetDirectoryName(idxPath) ?? string.Empty);
            if (Data.Any(x => x.romfs == fullPath)) return;
            var set = (Options.TinyLINKDATA ? new TinyDATA0(idxPath) : new DATA0(idxPath), File.OpenRead(binPath), fullPath, Path.GetFileNameWithoutExtension(idxPath));
            Data.Add(set);
            if (Data.Count == 1)
                RootEntryCount = set.Item1.Entries.Count + 1; // thanks Koei.
            else
                DataEntryCount = Data.Skip(1).Max(x => x.DATA0.Entries.Count);
        }

        /// <summary>
        ///     Adds a patch container.
        /// </summary>
        /// <param name="info0Path"></param>
        /// <param name="info1Path"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void AddInfoFSInternal(string info0Path, string info1Path)
        {
            PatchRomFS = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(info0Path) ?? string.Empty, ".."));
            Patch = (new INFO0(info0Path), new INFO1(info1Path));
            PatchEntryCount = Patch.INFO1.Entries.Count;
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

        /// <summary>
        ///     Load LINKDATA filename patterns
        /// </summary>
        /// <param name="filename"></param>
        public void LoadPatterns(string? filename = null) => Patterns = ManagedFSHelper.GetFileList(ManagedFSHelper.GetFileListLocation(filename, "LINKDATAPatterns", "link"), 4).Select(x => (x[0], x[2], x[1].ToCharArray(), x[3])).ToList();
    }
}
