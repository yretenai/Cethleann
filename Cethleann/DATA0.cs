using Cethleann.Structure;
using System;
using System.Collections.Generic;
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
                if (!stream.CanRead)
                {
                    throw new InvalidOperationException("Cannot read from stream!");
                }

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
        public Memory<byte> ReadEntry(Stream DATA1, int index)
        {
            if (index < 0 || index > Entries.Count)
            {
                throw new IndexOutOfRangeException($"Index {index} does not exist!");
            }

            return ReadEntry(DATA1, Entries[index]);
        }

        /// <summary>
        /// Reads a file entry from DATA1
        /// </summary>
        /// <param name="DATA1">Binary Read-capable Stream of DATA1</param>
        /// <param name="entry">Entry to read</param>
        /// <returns>memory stream of uncompressed bytes</returns>
        public unsafe Memory<byte> ReadEntry(Stream DATA1, DATA0Entry entry)
        {
            if (!DATA1.CanRead)
            {
                throw new InvalidOperationException("Cannot read from stream!");
            }

            DATA1.Position = entry.Offset;
            var buffer = new Memory<byte>(new byte[entry.UncompressedSize]);
            if (!entry.IsCompressed)
            {
                DATA1.Read(buffer.Span);
                return buffer;
            }

            Span<byte> zBuffer = stackalloc byte[12];
            DATA1.Read(zBuffer);
            var compInfo = MemoryMarshal.Read<DATA1CompressionInfo>(zBuffer);
            Helper.Assert(compInfo.Unknown != -1, "Unknown is -1");
            var chunkSizeBuffer = new Span<byte>(new byte[4 * compInfo.ChunkCount]);
            DATA1.Read(chunkSizeBuffer);
            var chunkSizes = MemoryMarshal.Cast<byte, int>(chunkSizeBuffer);
            var cursor = 0;
            DATA1.Position = entry.Offset + (long)(Math.Ceiling((double)(DATA1.Position - entry.Offset) / 0x80) * 0x80);
            for (var i = 0; i < compInfo.ChunkCount; ++i)
            {
                try
                {
                    var chunkSize = chunkSizes[i];
                    Span<byte> tmp;
                    if (chunkSize + cursor == entry.UncompressedSize)
                    {
                        tmp = new Span<byte>(new byte[chunkSize]);
                        DATA1.Read(tmp);
                        tmp.CopyTo(buffer.Span.Slice(cursor));
                        cursor += tmp.Length;
                        continue;
                    }

                    tmp = new Span<byte>(new byte[chunkSize]);
                    DATA1.Read(tmp);
                    using var zMs = new MemoryStream(tmp.ToArray()) { Position = 6 };
                    using var zDs = new DeflateStream(zMs, CompressionMode.Decompress);
                    cursor += zDs.Read(buffer.Span.Slice(cursor));
                }
                finally
                {
                    DATA1.Position = entry.Offset + (long)(Math.Ceiling((double)(DATA1.Position - entry.Offset) / 0x80) * 0x80);
                }
            }
            return buffer;
        }
    }
}
