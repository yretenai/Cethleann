using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cethleann.Structure;

namespace Cethleann.ManagedFS
{
    internal static class ManagedFSHelper
    {
        public static string[][] GetFileList(string loc, int fields)
        {
            return !File.Exists(loc) ? new string[0][] : File.ReadAllLines(loc).Select(x => x.Trim()).Where(x => x.Contains(",") && !x.StartsWith(";")).Select(x => x.Split(',', fields).Select(y => y.Trim()).ToArray()).ToArray();
        }

        public static string GetFileListLocation(string filename, DataGame game, string system)
        {
            return GetFileListLocation(filename, game == DataGame.None ? "" : game.ToString(), system);
        }

        public static string GetFileListLocation(string filename, string type, string system)
        {
            if (!string.IsNullOrWhiteSpace(filename) && File.Exists(filename)) return Path.GetFullPath(filename);
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename ?? $"filelist{(type?.Length == 0 ? "" : $"-{type}")}-{system}.csv");
        }

        public static Dictionary<string, string> GetSimpleFileList(string filename, DataGame game, string system)
        {
            var loc = GetFileListLocation(filename, game, system);
            if (!File.Exists(loc)) return new Dictionary<string, string>();
            var csv = GetFileList(loc, 2);
            return csv.ToDictionary(x => x[0], y => y[1]);
        }

        public static Dictionary<string, string> GetNamedFileList(string filename, DataGame game, string system)
        {
            var loc = GetFileListLocation(filename, game, system);
            if (!File.Exists(loc)) return new Dictionary<string, string>();
            var csv = GetFileList(loc, 3);
            return csv.ToDictionary(x => x[1], y => y[2]);
        }
    }
}
