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

namespace Cethleann
{
    /// <summary>
    ///     Management class for DATA0 and INFO0 files.
    /// </summary>
    public class Cethleann : IDisposable
    {
        /// <summary>
        ///     Loaded FileList.csv
        /// </summary>
        public Dictionary<int, string> FileList = new Dictionary<int, string>();

        /// <summary>
        ///     Loads data
        /// </summary>
        /// <param name="baseRomFs"></param>
        public Cethleann(string baseRomFs)
        {
            AddDataFS(baseRomFs);
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
        public List<(INFO0 INFO0, INFO1 INFO1, INFO2 INFO2)> Patch { get; private set; } = new List<(INFO0 INFO0, INFO1 INFO1, INFO2 INFO2)>();

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

        /// <summary>
        ///     Cleanup
        /// </summary>
        ~Cethleann()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Adds a DATA0 container, usually DLC
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void AddDataFS(string path)
        {
            var data0Path = Path.Combine(path, "DATA0.bin");
            var data1Path = Path.Combine(path, "DATA1.bin");
            if (!File.Exists(data0Path) || !File.Exists(data1Path)) throw new FileNotFoundException("DATA0 or DATA1 is missing from path");

            GC.ReRegisterForFinalize(this);

            var fullPath = Path.GetFullPath(path);
            if (Data.Any(x => x.romfs == fullPath)) return;
            var set = (new DATA0(data0Path), File.OpenRead(data1Path), fullPath);
            if (RootData == default)
            {
                RootData = set;
                RootEntryCount = RootData.DATA0.Entries.Count;
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
            Patch = new List<(INFO0 INFO0, INFO1 INFO1, INFO2 INFO2)>();
            PatchRomFS = path;
            var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<INFO2>()]);
            foreach (var directory in Directory.GetDirectories(path).Where(x => Path.GetFileName(x).StartsWith("patch", StringComparison.InvariantCultureIgnoreCase)).OrderBy(x => int.Parse(Path.GetFileName(x).Substring(5))))
            {
                var info0Path = Path.Combine(directory, "INFO0.bin");
                var info1Path = Path.Combine(directory, "INFO1.bin");
                var info2Path = Path.Combine(directory, "INFO2.bin");
                if (!File.Exists(info0Path) || !File.Exists(info1Path) || !File.Exists(info2Path)) continue;
                using var info2 = File.OpenRead(info2Path);
                info2.Read(buffer);
                var INFO2 = MemoryMarshal.Read<INFO2>(buffer);
                Patch.Add((new INFO0(INFO2, info0Path), new INFO1(INFO2, info1Path), INFO2));
            }
            PatchEntryCount = Patch.LastOrDefault().INFO1?.Entries.Count ?? 0;
        }

        /// <summary>
        ///     Reads an entry from the first valid (non-zero) storage.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Memory<byte> ReadEntry(int index, CethleannSearchFlags flags = CethleannSearchFlags.All)
        {
            if (index >= EntryCount) throw new IndexOutOfRangeException($"Index {index} does not exist!");
            
            if (index >= RootEntryCount)
            {
                index -= RootEntryCount;
                if (index >= PatchEntryCount)
                {
                    index -= PatchEntryCount;
                    return FindDLC(index, flags);
                }
                else
                {
                    return FindPatch(index, flags);
                }
            }

            if ((flags & CethleannSearchFlags.AllPatch) != CethleannSearchFlags.None)
                for (int i = Patch.Count; i > 0; --i)
                {
                    try
                    {
                        var (info0, _, _) = Patch[i - 1];
                        if (!flags.HasFlag((CethleannSearchFlags) (i << 16))) continue;

                        var entry = info0.ReadEntry(PatchRomFS, index);
                        if (entry.Length > 0) return entry;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // ignored.
                    }
                }

            if (!flags.HasFlag(CethleannSearchFlags.Base)) return Memory<byte>.Empty;

            var (baseData, baseStream, _) = RootData;
            return baseData.ReadEntry(baseStream, index);
        }

        private Memory<byte> FindPatch(int index, CethleannSearchFlags flags)
        {
            if ((flags & CethleannSearchFlags.AllPatch) == CethleannSearchFlags.None) return Memory<byte>.Empty;

            for (int i = Patch.Count; i > 0; --i)
            {
                try
                {
                    var (_, info1, _) = Patch[i - 1];
                    if (!flags.HasFlag((CethleannSearchFlags) (i << 16))) continue;

                    var entry = info1.ReadEntry(PatchRomFS, index);
                    if (entry.Length > 0) return entry;
                }
                catch (IndexOutOfRangeException)
                {
                    // ignored.
                }
            }

            return Memory<byte>.Empty;
        }

        private Memory<byte> FindDLC(int index, CethleannSearchFlags flags)
        {
            if ((flags & CethleannSearchFlags.AllDLC) == CethleannSearchFlags.None) return Memory<byte>.Empty;
            for (int i = 0; i < Data.Count; ++i)
            {
                var (data, stream, _) = Data[i];
                try
                {
                    if (flags.HasFlag((CethleannSearchFlags) (i << 4)))
                    {
                        var entry = data.ReadEntry(stream, index);
                        if (entry.Length > 0) return entry;
                    }
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

        /// <summary>
        ///     Loads a filelist from a csv file
        /// </summary>
        public void LoadFileList()
        {
            var loc = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "filelist.csv");
            if (!File.Exists(loc)) return;
            var csv = File.ReadAllLines(loc).Select(x => x.Trim()).Where(x => x.Contains(",") && !x.StartsWith("#")).Select(x => x.Split(',', 2, StringSplitOptions.RemoveEmptyEntries).Select(y => y.Trim()).ToArray());
            foreach (var entry in csv)
            {
                if (entry.Length < 2) continue;
                if (entry[0].Length == 0 || entry[1].Length == 0) continue;
                if (!int.TryParse(entry[0], out var id))
                {
                    Logger.Error("CETH", $"Id {id} cannot be parsed!");
                    continue;
                }

                if (Path.GetInvalidPathChars().Any(x => entry[1].Contains(x)))
                {
                    Logger.Error("CETH", $"Path {entry[1]} for id {id} is invalid!");
                    continue;
                }

                Logger.Assert(!FileList.ContainsKey(id), "!FileList.ContainsKey(id)", $"File List lists id {id} twice!");
                FileList[id] = entry[1];
            }
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
            if (dataType == DataType.Compressed || dataType == DataType.CompressedChonky) ext += ".gz";

            string path;
            var prefix = index >= RootEntryCount ? (index >= RootEntryCount + PatchEntryCount ? "dlc/" : "patch/") : "";

            if (index >= RootEntryCount && index < RootEntryCount + PatchEntryCount && Patch.Count > 0)
            {
                var info1 = Patch.Last().INFO1;
                path = info1.GetPath(index - RootEntryCount);
                if (!string.IsNullOrWhiteSpace(path) && path != "nx/")
                {
                    if (path.StartsWith("nx/", StringComparison.InvariantCultureIgnoreCase)) path = path.Substring(3);
                    var dir = Path.GetDirectoryName(path);
                    var file = Path.GetFileName(path);
                    path = "patch/" + Path.Combine(dir, $"{index} - {file}");
                    return prefix + (ext.EndsWith(".gz") ? path + ".gz" : path);
                }
            }

            if (FileList.TryGetValue(index, out path)) return prefix + (ext.EndsWith(".gz") ? path + ".gz" : path);
            return prefix + (ext == "bin" || ext == "bin.gz" ? $"misc/unknown/{index}.bin" : $"misc/formats/{ext.ToUpper().Replace('.', '_')}/{index}.{ext}");
        }
    }
}
