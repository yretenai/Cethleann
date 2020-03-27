using System;
using System.Collections.Generic;
using System.Linq;
using Cethleann.Archive;
using Cethleann.ManagedFS;
using Cethleann.Structure.KTID;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    /// <summary>
    ///     Parser for RDB INFO lists (NDB)
    /// </summary>
    [PublicAPI]
    public class RDBINFO
    {
        /// <summary>
        ///     Initialize with no data (for compatability)
        /// </summary>
        public RDBINFO() { }

        /// <summary>
        ///     Initialize with stream data
        /// </summary>
        /// <param name="buffer"></param>
        public RDBINFO(Span<byte> buffer)
        {
            //                                                         0      1     2      3      4    5
            var csv = ManagedFSHelper.GetFileList(buffer); // Format,Index,Offset,Length,Name,Path
            foreach (var line in csv)
            {
                if (line[4].Length == 0) continue;
                var ktid = RDB.Hash(line[4]);
                Entries.Add((ktid, line));

                var (name, ext) = RDB.StripName(line[4]);
                NameMap[ktid] = name;
                HashMap[ktid] = line[4];
            }
        }

        /// <summary>
        ///     Lsof entries with strings
        /// </summary>
        public List<(KTIDReference reference, string[] strings)> Entries { get; set; } = new List<(KTIDReference reference, string[] strings)>();

        /// <summary>
        ///     Hashes mapped to strings
        /// </summary>
        public Dictionary<KTIDReference, string> NameMap { get; set; } = new Dictionary<KTIDReference, string>();

        /// <summary>
        ///     Hashes of both names and typeinfos.
        /// </summary>
        public Dictionary<KTIDReference, string> HashMap { get; set; } = new Dictionary<KTIDReference, string>();

        /// <summary>
        ///     Type infos hashes mapped to guessed extensions
        /// </summary>
        public Dictionary<KTIDReference, string> ExtMap { get; set; } = new Dictionary<KTIDReference, string>();

        /// <summary>
        ///     Wild if true.
        /// </summary>
        public bool IsEmpty => Entries.Count == 0;

        /// <summary>
        ///     Merges one RDBINFO with another.
        /// </summary>
        /// <param name="other"></param>
        public void Union(RDBINFO other)
        {
            Entries = Entries.Union(other.Entries).Distinct().ToList();
            NameMap = NameMap.Union(other.NameMap).Distinct().ToDictionary(x => x.Key, y => y.Value);
            HashMap = HashMap.Union(other.HashMap).Distinct().ToDictionary(x => x.Key, y => y.Value);
            ExtMap = ExtMap.Union(other.ExtMap).Distinct().ToDictionary(x => x.Key, y => y.Value);
        }
    }
}
