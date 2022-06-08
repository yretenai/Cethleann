using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Cethleann.Archive;
using Cethleann.ManagedFS.Options;
using Cethleann.ManagedFS.Options.Default;
using Cethleann.Structure;
using Cethleann.Structure.KTID;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    [PublicAPI]
    public sealed class Edelgard : IManagedFS
    {
        public Edelgard(IManagedFSOptionsBase options)
        {
            if (options is IManagedFSOptions optionsLayer) GameId = optionsLayer.GameId;
            if (options is INyotenguOptions nyotenguOptions) Options = nyotenguOptions;
        }

        public INyotenguOptions Options { get; set; } = new NyotenguOptions();
        private Dictionary<KTIDReference, string> ExtList { get; set; } = new Dictionary<KTIDReference, string>();

        public List<RDX> RDXs { get; set; } = new List<RDX>();

        public Dictionary<KTIDReference, string> FileList { get; set; } = new Dictionary<KTIDReference, string>();

        public int EntryCount { get; set; }

        public string GameId { get; } = "";

        public Memory<byte> ReadEntry(int index)
        {
            if (index < 0) return Memory<byte>.Empty;

            foreach (var rdx in RDXs)
            {
                if (index < rdx.Entries.Count) 
                    return rdx.ReadEntry(index);

                index -= rdx.Entries.Count;
            }

            return Memory<byte>.Empty;
        }

        public string? GetFilename(int index, string ext = "bin", DataType dataType = DataType.None) => GetFilenameInternal(index);

        public void AddDataFS(string path)
        {
            Logger.Success("Edelgard", $"Loading {Path.GetFileName(path)}...");
            var rdx = new RDX(File.ReadAllBytes(path), Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path) ?? string.Empty);
            EntryCount += rdx.Entries.Count;
            RDXs.Add(rdx);
        }

        public Dictionary<string, string> LoadFileList(string? filename = null, string? game = null)
        {
            FileList = LoadKTIDFileListShared(filename, game ?? GameId);
            return FileList.ToDictionary(x => x.Key.ToString(CultureInfo.InvariantCulture), y => y.Value);
        }

        public Memory<byte> ReadEntry(KTIDReference ktid)
        {
            foreach (var rdx in RDXs)
            {
                if (rdx.KTIDToEntryId.TryGetValue(ktid, out var index))
                {
                    return ReadEntry(index);
                }
            }

            return Memory<byte>.Empty;
        }

        public static Dictionary<KTIDReference, string> LoadKTIDFileListShared(string? filename = null, string game = "")
        {
            Dictionary<KTIDReference, string> dictionary = new Dictionary<KTIDReference, string>();
            foreach (var (key, value) in LoadKTIDFileList(filename, game).Concat(LoadKTIDFileList(filename, "RDXShared"))) dictionary[key] = value;
            return dictionary;
        }

        public static Dictionary<KTIDReference, string> LoadKTIDFileList(string? filename = null, string game = "")
        {
            var loc = ManagedFSHelper.GetFileListLocation(filename, game, "rdb");
            var csv = ManagedFSHelper.GetFileList(loc, 3);
            var fileList = new Dictionary<KTIDReference, string>();
            foreach (var (key, value) in csv.Select(x => (key: KTIDReference.Parse(x[1].ToLower(), NumberStyles.HexNumber), value: x[2]))) fileList[key] = value;

            return fileList;
        }

        public static Dictionary<KTIDReference, (string, string)> LoadKTIDFileListEx(string? filename = null, string game = "")
        {
            var loc = ManagedFSHelper.GetFileListLocation(filename, game, "rdb");
            var csv = ManagedFSHelper.GetFileList(loc, 3);
            var fileList = new Dictionary<KTIDReference, (string, string)>();
            foreach (var (key, value) in csv.Select(x => (key: KTIDReference.Parse(x[1].ToLower(), NumberStyles.HexNumber), value: (x[0], x[2]))))
            {
                if (fileList.ContainsKey(key)) Logger.Warn("NYO", $"File List contains filename hash twice! ({key}, {value}, {fileList[key]})");
                fileList[key] = value;
            }

            return fileList;
        }

        private string? GetFilenameInternal(int index)
        {
            var prefix = string.Empty;
            var entry = default(RDBEntry);
            var selectedRdx = default(RDX);
            foreach (var rdx in RDXs)
            {
                if (index >= rdx.Entries.Count)
                {
                    index -= rdx.Entries.Count;
                    continue;
                }

                prefix = rdx.Name;
                entry = rdx.Entries[index];
                selectedRdx = rdx;
                break;
            }

            if (selectedRdx == null) return null;

            if (!selectedRdx.NameDatabase.ExtMap.TryGetValue(entry.TypeInfoKTID, out var ext) && (!ExtList.TryGetValue(entry.TypeInfoKTID, out ext) || string.IsNullOrEmpty(ext))) ext = selectedRdx.NameDatabase.HashMap.TryGetValue(entry.TypeInfoKTID, out ext) ? ext.Split(':').Last() : entry.TypeInfoKTID.ToString("x8");

            prefix += $@"\{ext}";

            if ((!selectedRdx.NameDatabase.NameMap.TryGetValue(entry.FileKTID, out var path) || string.IsNullOrWhiteSpace(path)) && (!FileList.TryGetValue(entry.FileKTID, out path) || string.IsNullOrWhiteSpace(path)))
            {
                path = $"{entry.FileKTID:x8}.{ext}";
            }
            else
            {
                if (string.IsNullOrEmpty(Path.GetExtension(path))) path += $".{ext}";
                if (Options.RDBPrefixFilenames) path = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, $"{entry.FileKTID:x8}{RDB.HASH_PREFIX_STR}{Path.GetFileNameWithoutExtension(path)}{RDB.HASH_SUFFIX_STR}.{Path.GetExtension(path)}");
            }

            return $@"{prefix}\{path}";
        }
        
        public void LoadExtList(string? filename = null) => ExtList = ManagedFSHelper.GetSimpleFileList(ManagedFSHelper.GetFileListLocation(filename, "RDBExt", "rdb"), "", "rdb").ToDictionary(x => KTIDReference.Parse(x.Key, NumberStyles.HexNumber), y => y.Value);


        public void Dispose()
        {
            RDXs.Clear();
            EntryCount = 0;
        }

        public void SaveGeneratedFileList(string? filename = null, string? game = null)
        {
            var filelist = LoadKTIDFileListEx(filename, game ?? GameId);
            foreach (var rdb in RDXs)
            foreach (var (hash, name) in rdb.NameDatabase.NameMap)
                filelist[hash] = (rdb.Name, name);

            SaveGeneratedFileList(filelist, filename, game ?? GameId);
        }

        public static void SaveGeneratedFileList(Dictionary<KTIDReference, (string, string)> filelist, string? filename = null, string game = "")
        {
            Logger.Debug("Nyotengu", $"Filelist saved to {ManagedFSHelper.GetFileListLocation(filename, game, "rdb-generated")}");
            File.WriteAllText(ManagedFSHelper.GetFileListLocation(filename, game, "rdb-generated"), string.Join("\n", filelist.OrderBy(x => $"{x.Value.Item1}{x.Key:x8}").Select(x => $"{x.Value.Item1},{x.Key:x8},{x.Value.Item2}")));
        }
    }
}
