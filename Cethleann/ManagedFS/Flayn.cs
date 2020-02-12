using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Koei;
using Cethleann.Structure;
using DragonLib;
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
        public (DATA0 DATA0, Stream DATA1, string romfs) RootData { get; private set; }

        /// <summary>
        ///     DLC data
        /// </summary>
        public List<(DATA0 DATA0, Stream DATA1, string romfs)> Data { get; private set; } = new List<(DATA0 DATA0, Stream DATA1, string romfs)>();

        /// <summary>
        ///     Patch data
        /// </summary>
        public (INFO0 INFO0, INFO1 INFO1, INFO2 INFO2) Patch { get; private set; }

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
                index -= RootEntryCount;
                if (index < PatchEntryCount) return FindPatch(index);

                index -= PatchEntryCount;
                return FindDLC(index);
            }

            try
            {
                var (info0, _, _) = Patch;
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

            var (baseData, baseStream, _) = RootData;
            return baseData.ReadEntry(baseStream, index);
        }

        /// <inheritdoc />
        public Dictionary<string, string> LoadFileList(string filename = null, DataGame? game = null)
        {
            FileList = ManagedFSHelpers.GetSimpleFileList(filename, game ?? GameId, "link");
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
                if (index >= RootEntryCount + PatchEntryCount)
                {
                    prefix = "dlc/";
                    logicalId = $"DLC_{index - RootEntryCount - PatchEntryCount} - {index}";
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
            AddLinkFS(path, "DATA0", "DATA1");
        }

        /// <summary>
        ///     Cleanup
        /// </summary>
        ~Flayn()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Adds a LINKDATA container, usually base games
        /// </summary>
        /// <param name="path"></param>
        /// <param name="idxHint"></param>
        /// <param name="binHint"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void AddLinkFS(string path, string idxHint, string binHint = null)
        {
            var idxPath = Path.Combine(path, $"{idxHint}.IDX");
            if (!File.Exists(idxPath)) idxPath = Path.Combine(path, $"{idxHint}.BIN");
            var binPath = Path.Combine(path, $"{binHint ?? idxHint}.BIN");
            if (!File.Exists(idxPath) || !File.Exists(binPath)) throw new FileNotFoundException("Cannot find DATA or LINKDATA pairs");

            Name = idxHint;

            AddDataFSInternal(idxPath, binPath);
        }

        private void AddDataFSInternal(string idxPath, string binPath)
        {
            GC.ReRegisterForFinalize(this);

            var fullPath = Path.GetFullPath(Path.GetDirectoryName(idxPath));
            if (Data.Any(x => x.romfs == fullPath)) return;
            var set = (new DATA0(idxPath), File.OpenRead(binPath), fullPath);
            if (RootData == default)
            {
                RootData = set;
                RootEntryCount = RootData.DATA0.Entries.Count + 1; // thanks Koei.
            }
            else
            {
                Data.Add(set);
                DataEntryCount = Data.Max(x => x.DATA0.Entries.Count);
            }
        }

        /// <summary>
        ///     Adds a patch container.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void AddPatchFS(string path)
        {
            if (!Directory.Exists(path)) return;
            PatchRomFS = Path.GetFullPath(Path.Combine(path, ".."));
            var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<INFO2>()]);
            var info0Path = Path.Combine(path, "INFO0.bin");
            if (!File.Exists(info0Path)) return;
            var info1Path = Path.Combine(path, "INFO1.bin");
            var info2Path = Path.Combine(path, "INFO2.bin");
            using var info2Stream = File.OpenRead(info2Path);
            info2Stream.Read(buffer);
            var info2 = MemoryMarshal.Read<INFO2>(buffer);
            Patch = (new INFO0(info2, info0Path), new INFO1(info2, info1Path), info2);
            PatchEntryCount = Patch.INFO1?.Entries.Count ?? 0;
        }

        private Memory<byte> FindPatch(int index)
        {
            try
            {
                var (_, info1, _) = Patch;
                var entry = info1.ReadEntry(PatchRomFS, index);
                if (entry.Length > 0) return entry;
            }
            catch (IndexOutOfRangeException)
            {
                // ignored.
            }

            return Memory<byte>.Empty;
        }

        private Memory<byte> FindDLC(int index)
        {
            foreach (var (data, stream, _) in Data)
            {
                if (index >= data.Entries.Count)
                {
                    index -= data.Entries.Count;
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

        private void Dispose(bool disposing)
        {
            foreach (var (_, stream, _) in Data) stream.Dispose();
            if (!disposing) return;
            Data = new List<(DATA0 DATA0, Stream DATA1, string romfs)>();
            DataEntryCount = 0;
            PatchEntryCount = 0;
            RootEntryCount = 0;
        }
    }
}
