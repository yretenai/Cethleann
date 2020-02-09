using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cethleann.Structure;

namespace Cethleann.ManagedFS
{
    internal static class ManagedFSHelpers
    {
        public static string[][] GetFileList(string loc, int fields)
        {
            return !File.Exists(loc) ? new string[0][] : File.ReadAllLines(loc).Select(x => x.Trim()).Where(x => x.Contains(",") && !x.StartsWith(";")).Select(x => x.Split(',', fields, StringSplitOptions.RemoveEmptyEntries).Select(y => y.Trim()).ToArray()).ToArray();
        }

        public static string GetFileListLocation(string filename, DataGame game)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename ?? $"filelist{(game == DataGame.None ? "" : $"-{game:G}")}.csv");
        }

        public static string GetFileListLocation(string filename, string type)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename ?? $"filelist{(type?.Length == 0 ? "" : $"-{type}")}.csv");
        }

        public static Dictionary<string, string> GetSimpleFileList(string filename, DataGame game)
        {
            var loc = GetFileListLocation(filename, game);
            if (!File.Exists(loc)) return new Dictionary<string, string>();
            var csv = GetFileList(loc, 2);
            return csv.ToDictionary(x => x[0], y => y[1]);
        }
    }
}
