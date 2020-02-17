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
    ///     INFO0 is a list of information for which to read the patch RomFS with.
    /// </summary>
    [PublicAPI]
    public class INFO0
    {
        /// <summary>
        ///     Reads a INFO0 file list from a path
        /// </summary>
        /// <param name="path">File path to read</param>
#pragma warning disable IDE0068 // Use recommended dispose pattern, reason: disposed in sub-method DATA0(Stream, bool) when bool leaveOpen is false.
        public INFO0(string path) : this(File.OpenRead(path)) { }
#pragma warning restore IDE0068 // Use recommended dispose pattern

        /// <summary>
        ///     Reads a INFO0 file list from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="leaveOpen"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public INFO0(Stream stream, bool leaveOpen = false)
        {
            try
            {
                if (!stream.CanRead) throw new InvalidOperationException("Cannot read from stream!");

                var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<INFO0Entry>() + 0x100]);
                var entryCount = (int) (stream.Length / buffer.Length);
                Entries = new List<(INFO0Entry entry, string path)>(entryCount);
                for (int i = 0; i < entryCount; ++i)
                {
                    stream.Read(buffer);
                    var entry = MemoryMarshal.Read<INFO0Entry>(buffer);
                    var path = buffer.Slice(SizeHelper.SizeOf<INFO0Entry>()).ReadString(returnNull: false);
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
        ///     Entries found in the INFO0
        /// </summary>
        public List<(INFO0Entry entry, string path)> Entries { get; }

        /// <summary>
        ///     Attempts to read a Patch ROMFS entry
        /// </summary>
        /// <param name="romfs"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Memory<byte> ReadEntry(string romfs, int index)
        {
            var (entry, path) = Entries.FirstOrDefault(x => x.entry.Index == index);
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
            var (_, path) = Entries.FirstOrDefault(x => x.entry.Index == index);
            return path?.Substring(12, path.Length - 12 - (path.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase) ? 3 : 0));
        }

        /// <summary>
        ///     Attempts to read a Patch ROMFS entry
        /// </summary>
        /// <param name="entryPath"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static Memory<byte> ReadEntry(string entryPath, INFO0Entry entry)
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
