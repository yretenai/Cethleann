using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Compression;
using Cethleann.Structure;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Koei
{
    /// <summary>
    ///     INFO1 is a list of information of base files deleted with this patch.
    ///     Usually this means that the files are contained in a DLC.
    /// </summary>
    [PublicAPI]
    public class INFO1
    {
        /// <summary>
        ///     Reads a INFO1 file list from a stream
        /// </summary>
        /// <param name="info2"></param>
        /// <param name="stream"></param>
        /// <param name="leaveOpen"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public INFO1(INFO2 info2, Stream stream, bool leaveOpen = false)
        {
            try
            {
                if (!stream.CanRead) throw new InvalidOperationException("Cannot read from stream!");

                Entries = new List<(INFO1Entry entry, string path)>((int) info2.INFO1Count);
                var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<INFO1Entry>() + 0x100]);
                for (int i = 0; i < info2.INFO1Count; ++i)
                {
                    stream.Read(buffer);
                    var entry = MemoryMarshal.Read<INFO1Entry>(buffer);
                    var path = buffer.Slice(SizeHelper.SizeOf<INFO1Entry>()).ReadString(returnNull: false);
                    Entries.Add((entry, path));
                }
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        ///     Reads a INFO1 file list from a path
        /// </summary>
        /// <param name="info2"></param>
        /// <param name="path">File path to read</param>
#pragma warning disable IDE0068 // Use recommended dispose pattern, reason: disposed in sub-method DATA0(Stream, bool) when bool leaveOpen is false.
        public INFO1(INFO2 info2, string path) : this(info2, File.OpenRead(path)) { }
#pragma warning restore IDE0068 // Use recommended dispose pattern

        /// <summary>
        ///     Entries found in the INFO1
        /// </summary>
        public List<(INFO1Entry entry, string path)> Entries { get; }

        /// <summary>
        ///     Attempts to read a Patch ROMFS entry
        /// </summary>
        /// <param name="romfs"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Memory<byte> ReadEntry(string romfs, int index)
        {
            var (entry, path) = Entries.ElementAtOrDefault(index);
            if (path == null) throw new IndexOutOfRangeException($"Index {index} does not exist!");
            return ReadEntry(Path.Combine(romfs, path.Substring(5)), entry);
        }

        /// <summary>
        ///     Gets the path from Patch containers.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetPath(int index)
        {
            var (_, path) = Entries.ElementAtOrDefault(index);
            return path?.Substring(12, path.Length - 12 - (path.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase) ? 3 : 0));
        }

        /// <summary>
        ///     Attempts to read a Patch ROMFS entry
        /// </summary>
        /// <param name="entryPath"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static Memory<byte> ReadEntry(string entryPath, INFO1Entry entry)
        {
            if (entry.UncompressedSize == 0 || !File.Exists(entryPath)) return Memory<byte>.Empty;
            var buffer = new Memory<byte>(new byte[entry.UncompressedSize]);
            using var stream = File.OpenRead(entryPath);

            if (entry.IsCompressed != 0) return TableCompression.Decompress(stream, entry.CompressedSize);

            stream.Read(buffer.Span);
            return buffer;
        }
    }
}
