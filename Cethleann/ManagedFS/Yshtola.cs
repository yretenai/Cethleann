using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cethleann.ManagedFS.Support;
using Cethleann.Ninja;
using Cethleann.Structure;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Manages TN files
    /// </summary>
    // TODO: Migrate to IManagedFS
    [PublicAPI]
    public class Yshtola : IEnumerable<IDTableEntry>
    {
        /// <summary>
        ///     Initialize with standard data.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="settings"></param>
        public Yshtola(string root, YshtolaSettings settings)
        {
            Settings = settings;
            if (File.Exists(Path.Combine(root, Settings.TableName)))
                Root = Path.GetFullPath(Path.Combine(root, ".."));
            else if (File.Exists(Path.Combine(root, Settings.Directory, Settings.TableName))) Root = Path.GetFullPath(Path.Combine(root));

            Table = new IDTable(File.ReadAllBytes(Path.Combine(root, Settings.Directory, settings.TableName)), IDTableFlags.Compressed | IDTableFlags.Encrypted, Settings.XorTruth, Settings.Multiplier, Settings.Divisor);
        }

        /// <summary>
        ///     Settings to use for decryption and loading.
        /// </summary>
        public YshtolaSettings Settings { get; }

        /// <summary>
        ///     ID Table
        /// </summary>
        public IDTable Table { get; set; }

        /// <summary>
        ///     Root directory, the one that contains COMMON.
        /// </summary>
        public string Root { get; set; }

        /// <inheritdoc />
        public IEnumerator<IDTableEntry> GetEnumerator()
        {
            return ((IEnumerable<IDTableEntry>) Table.Entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///     Reads a file from an ID Table entry.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public (Memory<byte> Data, string Path) ReadEntry(IDTableEntry entry)
        {
            var path = Path.Combine(Root, entry.Path(Table.Table, Table.Header.Offset));
            if (!File.Exists(path)) throw new FileNotFoundException(path);

            return (new Memory<byte>(Table.Read(File.ReadAllBytes(path), entry, Settings.XorTruth, Settings.Multiplier, Settings.Divisor).ToArray()), entry.OriginalPath(Table.Table, Table.Header.Offset));
        }
    }
}
