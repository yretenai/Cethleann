using System;
using System.Collections.Generic;
using System.IO;
using Cethleann.Archive;
using Cethleann.ManagedFS.Options;
using Cethleann.ManagedFS.Options.Default;
using Cethleann.Structure;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.ManagedFS
{
    /// <summary>
    ///     Manages PAK files
    /// </summary>
    [PublicAPI]
    public class Reisalin : IManagedFS
    {
        /// <summary>
        ///     Loads data
        /// </summary>
        /// <param name="options"></param>
        public Reisalin(IManagedFSOptionsBase options)
        {
            if (options is IManagedFSOptions optionsLayer) GameId = optionsLayer.GameId;
            if (options is IReisalinOptions reisalinOptions) Options = reisalinOptions;
        }

        /// <summary>
        ///     Reisalin specific options
        /// </summary>
        public IReisalinOptions Options { get; set; } = new ReisalinOptions();

        /// <summary>
        ///     Lsof PAKs mounted
        /// </summary>
        public List<PAK> PAKs { get; set; } = new List<PAK>();

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public int EntryCount { get; private set; }

        /// <inheritdoc />
        public string GameId { get; } = "";

        /// <inheritdoc />
        public Memory<byte> ReadEntry(int index)
        {
            foreach (var pak in PAKs)
            {
                if (index < pak.Entries.Count) return pak.ReadEntry(pak.Entries[index]);
                index -= pak.Entries.Count;
            }

            return Memory<byte>.Empty;
        }

        /// <inheritdoc />
        public Dictionary<string, string>? LoadFileList(string? filename = null, string? game = null) => null;

        /// <inheritdoc />
        public string? GetFilename(int index, string ext = "bin", DataType dataType = DataType.None)
        {
            foreach (var pak in PAKs)
            {
                if (index < pak.Entries.Count) return pak.Entries[index].Filename;
                index -= pak.Entries.Count;
            }

            return null;
        }

        /// <inheritdoc />
        public void AddDataFS(string path)
        {
            Logger.Success("Reisalin", $"Loading {Path.GetFileName(path)}...");
            var pak = new PAK(path, !Options.ReisalinA17);
            EntryCount += pak.Entries.Count;
            PAKs.Add(pak);
        }

        private void Dispose(bool disposing)
        {
            foreach (var pak in PAKs) pak.Dispose();
            if (!disposing) return;
            PAKs.Clear();
        }

        /// <summary>
        ///     Disposes
        /// </summary>
        ~Reisalin() => Dispose(false);
    }
}
