using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;
using DragonLib.IO;

namespace Cethleann
{
    public class Cethleann : IDisposable
    {
        public Dictionary<int, string> FileList = new Dictionary<int, string>();

        public Cethleann(string baseRomFs)
        {
            AddDataFS(baseRomFs);
        }

        public List<(DATA0 DATA0, Stream DATA1, string romfs)> Data { get; private set; } = new List<(DATA0 DATA0, Stream DATA1, string romfs)>();
        public List<(INFO0 INFO0, INFO1 INFO1, INFO2 INFO2)> Patch { get; private set; } = new List<(INFO0 INFO0, INFO1 INFO1, INFO2 INFO2)>();
        public string PatchRomFS { get; private set; }

        public int EntryCount => (int) Math.Max(Data.Count > 0 ? Data.Max(x => x.DATA0.Entries.Count) : 0, Patch.Count > 0 ? Patch.Max(x => x.INFO0.Entries.Max(y => y.entry.Index)) : 0);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Cethleann()
        {
            Dispose(false);
        }

        public void AddDataFS(string path)
        {
            var data0Path = Path.Combine(path, "DATA0.bin");
            var data1Path = Path.Combine(path, "DATA1.bin");
            if (!File.Exists(data0Path) || !File.Exists(data1Path)) throw new FileNotFoundException("DATA0 or DATA1 is missing from path");

            GC.ReRegisterForFinalize(this);

            var fullPath = Path.GetFullPath(path);
            if (Data.Any(x => x.romfs == fullPath)) return;
            Data.Add((new DATA0(data0Path), File.OpenRead(data1Path), fullPath));
        }

        public void AddPatchFS(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("Patch RomFS is not found!");
            Patch = new List<(INFO0 INFO0, INFO1 INFO1, INFO2 INFO2)>();
            PatchRomFS = path;
            var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<INFO2>()]);
            foreach (var directory in Directory.GetDirectories(path))
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
        }

        public Memory<byte> ReadEntry(int index, CethleannSearchFlags flags = CethleannSearchFlags.All)
        {
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

            if ((flags & CethleannSearchFlags.AllDLC) != CethleannSearchFlags.None)
                for (int i = 1; i < Data.Count; ++i)
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

            if (!flags.HasFlag(CethleannSearchFlags.Base) || Data.Count <= 0) return Memory<byte>.Empty;

            var (baseData, baseStream, _) = Data[0];
            return baseData.ReadEntry(baseStream, index);
        }

        public void Dispose(bool disposing)
        {
            foreach (var (_, stream, _) in Data) stream.Dispose();

            if (disposing) Data = new List<(DATA0 DATA0, Stream DATA1, string romfs)>();
        }

        public void LoadFileList()
        {
            var loc = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "filelist.csv");
            if (!File.Exists(loc)) return;
            var csv = File.ReadAllLines(loc).Select(x => x.Trim()).Where(x => x.Contains(",") && !x.StartsWith("#")).Select(x => x.Split(',', 2, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray());
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

        public string GetFilename(int index, string ext = "bin")
        {
            if (FileList.TryGetValue(index, out var path)) return path;
            return ext == "bin" ? $"misc/unknown/{index}.bin" : $"misc/formats/{ext.ToUpper().Replace('.', '_')}/{index}.{ext}";
        }
    }
}
