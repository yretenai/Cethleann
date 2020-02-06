using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cethleann.Structure;
using DragonLib.IO;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Generic FS Interface
    /// </summary>
    public interface IManagedFS : IDisposable
    {
        /// <summary>
        ///     Maximum number of entries found in both containers and patches
        /// </summary>
        int EntryCount { get; }

        /// <summary>
        ///     Game ID of the game.
        /// </summary>
        DataGame GameId { get; }

        /// <summary>
        ///     Reads an entry from the first valid (non-zero) storage.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Memory<byte> ReadEntry(int index); 
        
        /// <summary>
        ///     Filename List
        /// </summary>
        Dictionary<string, string> FileList { get; set; }

        /// <summary>
        ///     Loads a filelist from a csv file
        /// </summary>
        Dictionary<string, string> LoadFileList(string filename = null, DataGame game = DataGame.None)
        {
            if (game == DataGame.None) game = GameId;
            var loc = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename ?? $"filelist{(game == DataGame.None ? "" : $"-{game:G}")}.csv");
            if (!File.Exists(loc)) return null;
            var csv = File.ReadAllLines(loc).Select(x => x.Trim()).Where(x => x.Contains(",") && !x.StartsWith("#")).Select(x => x.Split(',', 2, StringSplitOptions.RemoveEmptyEntries).Select(y => y.Trim()).ToArray());
            var filelist = new Dictionary<string, string>();
            foreach (var entry in csv)
            {
                if (entry.Length < 2) continue;
                if (entry[0].Length == 0 || entry[1].Length == 0) continue;
                var id = entry[0];

                if (Path.GetInvalidPathChars().Any(x => entry[1].Contains(x)))
                {
                    Logger.Error("CETH", $"Path {entry[1]} for id {id} is invalid!");
                    continue;
                }

                Logger.Assert(!filelist.ContainsKey(id), "!FileList.ContainsKey(id)", $"File List lists id {id} twice!");
                filelist[id] = entry[1];
            }

            if (game == GameId) FileList = filelist;

            return filelist;
        }

        /// <summary>
        ///     Attempts to get a valid filename/filepath.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ext"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        string GetFilename(int index, string ext = "bin", DataType dataType = DataType.None);

        /// <summary>
        ///     Adds a container
        /// </summary>
        /// <param name="path"></param>
        void AddDataFS(string path);
    }
}
