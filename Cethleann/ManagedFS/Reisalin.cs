using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cethleann.Gust;
using Cethleann.Structure;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Manages PAK files
    /// </summary>
    [PublicAPI]
    public class Reisalin : IDisposable, IEnumerable<(PAKEntry, PAK)>
    {
        /// <summary>
        ///     Lsof PAKs mounted
        /// </summary>
        public List<PAK> PAKs { get; set; } = new List<PAK>();

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var pak in PAKs) pak.Dispose();
            PAKs.Clear();
        }

        /// <inheritdoc />
        public IEnumerator<(PAKEntry, PAK)> GetEnumerator()
        {
            return (from pak in PAKs from entry in pak.Entries select (entry, pak)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///     Mounts a PAK
        /// </summary>
        /// <param name="path"></param>
        public void Mount(string path)
        {
            if (Directory.Exists(path))
                foreach (var file in Directory.GetFiles(path, "*.PAK"))
                    Mount(file);
            else
                PAKs.Add(new PAK(path));
        }

        /// <summary>
        ///     Reads a file from a path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Memory<byte> ReadFile(string path)
        {
            foreach (var pak in PAKs)
                if (pak.TryGetEntry(path, out var entry))
                    return pak.ReadEntry(entry);

            throw new FileNotFoundException(path);
        }

        /// <summary>
        ///     Disposes
        /// </summary>
        ~Reisalin()
        {
            Dispose();
        }
    }
}
