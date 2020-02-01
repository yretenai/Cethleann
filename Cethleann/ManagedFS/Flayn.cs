using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Cethleann.Koei;
using Cethleann.Structure;
using DragonLib;
using DragonLib.IO;
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
        /// <param name="baseRomFs"></param>
        /// <param name="game"></param>
        public Flayn(string baseRomFs, DataGame game = DataGame.None)
        {
            GameId = game;
            AddDataFS(baseRomFs);
            if (GameId == DataGame.FireEmblemThreeHouses) Logger.Assert(RootEntryCount == 31161, "RootEntryCount == 31161");
        }

        /// <summary>
        ///     Loaded FileList.csv
        /// </summary>
        public Dictionary<string, string> FileList { get; } = new Dictionary<string, string>();

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
                var entry = info0.ReadEntry(PatchRomFS, index);
                if (entry.Length > 0) return entry;
            }
            catch (IndexOutOfRangeException)
            {
                // ignored.
            }

            var (baseData, baseStream, _) = RootData;
            return baseData.ReadEntry(baseStream, index);
        }

        /// <inheritdoc />
        public void LoadFileList(string filename = null)
        {
            var loc = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename ?? $"filelist{(GameId == DataGame.None ? "" : $"-{GameId:G}")}.csv");
            if (!File.Exists(loc)) return;
            var csv = File.ReadAllLines(loc).Select(x => x.Trim()).Where(x => x.Contains(",") && !x.StartsWith("#")).Select(x => x.Split(',', 2, StringSplitOptions.RemoveEmptyEntries).Select(y => y.Trim()).ToArray());
            foreach (var entry in csv)
            {
                if (entry.Length < 2) continue;
                if (entry[0].Length == 0 || entry[1].Length == 0) continue;
                var id = entry[0];

                if (Path.GetInvalidPathChars().Any(x => entry[1].Contains(x)))
                {
                    Logger.Error("FLAYN", $"Path {entry[1]} for id {id} is invalid!");
                    continue;
                }

                Logger.Assert(!FileList.ContainsKey(id), "!FileList.ContainsKey(id)", $"File List lists id {id} twice!");
                FileList[id] = entry[1];
            }
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
                    logicalId = $"DLC{index - RootEntryCount - PatchEntryCount} - {index}";
                }
                else
                {
                    prefix = "patch/";
                    logicalId = $"PATCH{index - RootEntryCount} - {index}";
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

            var temp = path;
            if (!FileList.TryGetValue(logicalId ?? index.ToString(), out path)) path = temp ?? (ext == "bin" || ext == "bin.gz" ? $"misc/unknown/{logicalId ?? index.ToString()}.{ext}" : $"misc/formats/{ext.ToUpper().Replace('.', '_')}/{logicalId ?? index.ToString()}.{ext}");
            else path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + $".{ext}");
            if (ext.EndsWith(".gz") && !path.EndsWith(".gz")) path += ".gz";
            return prefix + path;
        }

        /// <summary>
        ///     Cleanup
        /// </summary>
        ~Flayn()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Adds a DATA container, usually base games
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void AddDataFS(string path)
        {
            var data0Path = Path.Combine(path, "DATA0.bin");
            var data1Path = Path.Combine(path, "DATA1.bin");
            if (!File.Exists(data0Path) || !File.Exists(data1Path))
            {
                AddLinkFS(path);
                return;
            }

            AddDataFSInternal(data0Path, data1Path);
        }

        /// <summary>
        ///     Adds a LINKDATA container, usually base games
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void AddLinkFS(string path)
        {
            var idxPath = Path.Combine(path, "LINKDATA.IDX");
            var binPath = Path.Combine(path, "LINKDATA.BIN");
            if (!File.Exists(idxPath) || !File.Exists(binPath)) throw new FileNotFoundException("Cannot find DATA or LINKDATA pairs");

            if (GameId == DataGame.FireEmblemThreeHouses) GameId = DataGame.None;

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
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("Patch RomFS is not found!");
            PatchRomFS = Path.GetFullPath(Path.Combine(path, ".."));
            var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<INFO2>()]);
            var info0Path = Path.Combine(path, "INFO0.bin");
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

        /// <summary>
        ///     Disposes with Finalizer flag
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            foreach (var (_, stream, _) in Data) stream.Dispose();

            if (disposing) Data = new List<(DATA0 DATA0, Stream DATA1, string romfs)>();
        }

#if DEBUG
        /// <summary>
        ///     Tests DLC sanity. DEBUG
        /// </summary>
        public void TestDLCSanity()
        {
            if (Data.Count < 2) return;
            Logger.Assert(Data.All(x => x.DATA0.Entries.Count == DataEntryCount), "Data.All(x => x.DATA0.Entries.Count == DataEntryCount)");
            for (var i = 0; i < DataEntryCount; ++i) Logger.Assert(Data.Count(x => x.DATA0.Entries[i].UncompressedSize > 0) <= 1, "Data.Count(x => x.DATA0.Entries[i].UncompressedSize > 0) <= 1");
        }
#endif
    }
}
