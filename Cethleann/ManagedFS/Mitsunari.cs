using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cethleann.Ninja;
using Cethleann.Structure;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Nioh archive_*.bin file management
    /// </summary>
    [PublicAPI]
    public class Mitsunari : IManagedFS
    {
        /// <summary>
        ///     Loads data
        /// </summary>
        /// <param name="game"></param>
        public Mitsunari(DataGame game = DataGame.None)
        {
            GameId = game;
        }

        /// <summary>
        ///     Underlying LINKARCHIVEs
        /// </summary>
        public List<(LINKARCHIVE archive, LINKNAME name, string filename)> Data { get; set; } = new List<(LINKARCHIVE, LINKNAME, string)>();

        /// <summary>
        ///     Loaded FileList.csv
        /// </summary>
        public Dictionary<string, string> FileList { get; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public int EntryCount { get; private set; }

        /// <inheritdoc />
        public DataGame GameId { get; private set; }

        /// <inheritdoc />
        public Memory<byte> ReadEntry(int index)
        {
            if (index >= EntryCount) throw new IndexOutOfRangeException($"Index {index} does not exist!");
            foreach (var (archive, _, _) in Data)
            {
                if (index < archive.Entries.Length) return archive.ReadEntry(archive.Entries[index]);
                index -= archive.Entries.Length;
            }

            return Memory<byte>.Empty;
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
                    Logger.Error("MITSUNARI", $"Path {entry[1]} for id {id} is invalid!");
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

            var archiveName = "ARCHIVE_00";
            var linkName = default(LINKNAME);
            foreach (var (linkdata, name, archiveFilename) in Data)
            {
                if (index >= linkdata.Entries.Length)
                {
                    index -= linkdata.Entries.Length;
                    continue;
                }

                archiveName = archiveFilename.ToUpper();
                linkName = name;
                break;
            }

            var filename = linkName?.GetName(index) ?? index.ToString();
            if (!FileList.TryGetValue($"{archiveName}_{filename}", out var path))
            {
                if (Path.GetFileName(filename) == filename)
                {
                    filename = $"{index}_{filename}";
                    path = (ext == "bin" || ext == "bin.gz" ? $"MISC/UNKNOWN/{filename}.{ext}" : $"MISC/FORMATS/{ext.ToUpper().Replace('.', '_')}/{filename}.{ext}");
                }
                else
                {
                    path = filename;
                }
            }
            else
            {
                path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + $".{ext}");
            }

            if (ext.EndsWith(".gz") && !path.EndsWith(".gz")) path += ".gz";
            return @$"{archiveName}\{path}";
        }

        /// <inheritdoc />
        public void AddDataFS(string path)
        {
            GC.ReRegisterForFinalize(this);
            
            var archives = Directory.GetFiles(path, "*.lnk");
            foreach (var archive in archives)
            {
                var linkarchive = new LINKARCHIVE(File.OpenRead(archive));
                var name = Path.GetFileNameWithoutExtension(archive);
                var linkname = new LINKNAME(File.OpenRead(Path.Combine(path, "lfm_order_" + name.Substring(8) + ".bin")).ToSpan());
                Data.Add((linkarchive, linkname, name.ToUpper()));
                EntryCount += linkarchive.Entries.Length;
            }
        }

        /// <summary>
        ///     Disposes with Finalizer flag
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            foreach (var (archive, _, _) in Data) archive.Dispose();

            if (disposing) Data = new List<(LINKARCHIVE, LINKNAME, string)>();
        }

        /// <summary>
        ///     Cleanup
        /// </summary>
        ~Mitsunari()
        {
            Dispose(false);
        }
    }
}
