using System;
using System.Collections.Generic;
using System.Globalization;
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
                var encodedName = line[4];
                uint ktid;
                if (line.Length > 6) // .bin.info
                {
                    encodedName = line[2];
                    ktid = uint.Parse(line[1], NumberStyles.HexNumber);
                }
                else // .info
                {
                    if (encodedName.Length == 0) continue;
                    ktid = RDB.Hash(line[4]);
                }
                Entries.Add((ktid, line));

                var (name, _) = RDB.StripName(encodedName);
                NameMap[ktid] = name;
                HashMap[ktid] = encodedName;
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
            foreach (var entry in other.Entries)
            {
                Entries.Add(entry);
            }
            Entries = Entries.Distinct().ToList();
            foreach (var (key, value) in other.NameMap)
            {
                NameMap[key] = value;
            }
            foreach (var (key, value) in other.HashMap)
            {
                HashMap[key] = value;
            }
            foreach (var (key, value) in other.ExtMap)
            {
                ExtMap[key] = value;
            }
        }
    }
}
