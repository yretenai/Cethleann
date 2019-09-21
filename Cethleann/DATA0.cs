using Cethleann.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cethleann
{
    /// <summary>
    /// DATA0 is a list of information for which to read DATA1 with.
    /// </summary>
    public class DATA0
    {
        /// <summary>
        /// lsof <seealso cref="DATA0Entry"/>
        /// </summary>
        public List<DATA0Entry> Entries { get; set; }

        /// <summary>
        /// Reads a DATA0 file list from a path
        /// </summary>
        /// <param name="path">File path to read</param>
#pragma warning disable IDE0068 // Use recommended dispose pattern, reason: disposed in sub-method DATA0(Stream, bool) when bool leaveOpen is false.
        public DATA0(string path) : this(File.OpenRead(path), false) { }
#pragma warning restore IDE0068 // Use recommended dispose pattern

        /// <summary>
        /// Reads a DATA0 file list from a stream
        /// </summary>
        /// <param name="stream">Binary Read-capable Stream of DATA0</param>
        /// <param name="leaveOpen">If true, won't dispose <paramref name="stream"/></param>
        public DATA0(Stream stream, bool leaveOpen = false)
        {
            try
            {
                if (!stream.CanRead) throw new InvalidOperationException("Cannot read from stream!");
                Span<byte> buffer = stackalloc byte[32];
                var entries = new DATA0Entry[stream.Length / 32];
                var i = 0;
                while (stream.Read(buffer) == 32)
                {
                    entries[i++] = MemoryMarshal.Read<DATA0Entry>(buffer);
                }
                Entries = entries.ToList();
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
        /// Reads a file index from DATA1
        /// </summary>
        /// <param name="DATA1">Binary Read-capable Stream of DATA1</param>
        /// <param name="index">Entry Index to read</param>
        /// <returns>memory stream of uncompressed bytes</returns>
        public MemoryStream ReadEntry(Stream DATA1, int index)
        {
            if (index < 0 || index > Entries.Count) throw new IndexOutOfRangeException($"Index {index} does not exist!");
            return ReadEntry(DATA1, Entries[index]);
        }

        private static readonly uint[] dataTypes = Enum.GetValues(typeof(DataType)).Cast<uint>().ToArray();

        /// <summary>
        /// Guesses the format based on the magic value.
        /// </summary>
        /// <param name="data">data to test</param>
        /// <returns>data type, null if magic isn't known</returns>
        public static unsafe DataType? GuessType(Stream data)
        {
            Span<byte> buffer = stackalloc byte[4];
            data.Read(buffer);
            data.Position -= 4;
            return GuessType(buffer);
        }

        /// <summary>
        /// Guesses the format based on the magic value.
        /// </summary>
        /// <param name="buffer">data to test</param>
        /// <returns>data type, null if magic isn't known</returns>
        public static DataType? GuessType(Span<byte> buffer)
        {
            var magic = MemoryMarshal.Read<uint>(buffer);
            if (dataTypes.Contains(magic)) return (DataType)magic;
            return null;
        }

        /// <summary>
        /// Guesses if the stream is a DataTable
        /// </summary>
        /// <param name="data">data to test</param>
        /// <returns>true if the header is predictable</returns>
        public static unsafe bool GuessDataTable(Stream data)
        {
            Span<byte> buffer = stackalloc byte[8];
            data.Read(buffer);
            data.Position -= 8;
            return GuessDataTable(buffer);
        }

        /// <summary>
        /// Guesses if the stream is a DataTable
        /// </summary>
        /// <param name="buffer">data to test</param>
        /// <returns>true if the header is predictable</returns>
        public static bool GuessDataTable(Span<byte> buffer)
        {
            var count = MemoryMarshal.Read<uint>(buffer);
            var firstOffset = MemoryMarshal.Read<uint>(buffer.Slice(4));
            return firstOffset == 4 + count * 8;
        }

        /// <summary>
        /// Reads a file entry from DATA1
        /// </summary>
        /// <param name="DATA1">Binary Read-capable Stream of DATA1</param>
        /// <param name="entry">Entry to read</param>
        /// <returns>memory stream of uncompressed bytes</returns>
        public unsafe MemoryStream ReadEntry(Stream DATA1, DATA0Entry entry)
        {
            if (!DATA1.CanRead) throw new InvalidOperationException("Cannot read from stream!");
            DATA1.Position = entry.Offset;
            var buffer = new Span<byte>(new byte[entry.UncompressedSize]);
            if (!entry.IsCompressed)
            {
                DATA1.Read(buffer);
                return new MemoryStream(buffer.ToArray()) { Position = 0 };
            }

            Span<byte> zBuffer = stackalloc byte[12];
            DATA1.Read(zBuffer);
            var compInfo = MemoryMarshal.Read<DATA1CompressionInfo>(zBuffer);
#if DEBUG_ASSERTIONS
            Debug.Assert(compInfo.Unknown != -1, "Unknown is -1");
#endif
            var chunkSizeBuffer = new Span<byte>(new byte[4 * compInfo.ChunkCount]);
            DATA1.Read(chunkSizeBuffer);
            var chunkSizes = MemoryMarshal.Cast<byte, int>(chunkSizeBuffer);

            var ms = new MemoryStream() { Position = 0 };
            DATA1.Position = entry.Offset + (long)(Math.Ceiling((double)(DATA1.Position - entry.Offset) / 0x80) * 0x80);
            for (var i = 0; i < compInfo.ChunkCount; ++i)
            {
                try
                {
                    var chunkSize = chunkSizes[i];
                    Span<byte> tmp;
                    if (chunkSize + ms.Length == entry.UncompressedSize)
                    {
                        tmp = new Span<byte>(new byte[chunkSize]);
                        DATA1.Read(tmp);
                        ms.Write(tmp);
                        continue;
                    }

                    tmp = new Span<byte>(new byte[chunkSize]);
                    DATA1.Read(tmp);
                    using var zMs = new MemoryStream(tmp.ToArray()) { Position = 6 };
                    using var zDs = new DeflateStream(zMs, CompressionMode.Decompress);
                    zDs.CopyTo(ms);
                }
                finally
                {
                    DATA1.Position = entry.Offset + (long)(Math.Ceiling((double)(DATA1.Position - entry.Offset) / 0x80) * 0x80);
                }
            }
            ms.Position = 0;
            return ms;
        }
    }
}
