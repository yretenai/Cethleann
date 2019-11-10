using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;

namespace Cethleann
{
    /// <summary>
    ///     DATA0 is a list of information for which to read DATA1 with.
    /// </summary>
    public class DATA0
    {
        /// <summary>
        ///     Reads a DATA0 file list from a path
        /// </summary>
        /// <param name="path">File path to read</param>
#pragma warning disable IDE0068 // Use recommended dispose pattern, reason: disposed in sub-method DATA0(Stream, bool) when bool leaveOpen is false.
        public DATA0(string path) : this(File.OpenRead(path)) { }
#pragma warning restore IDE0068 // Use recommended dispose pattern

        /// <summary>
        ///     Reads a DATA0 file list from a stream
        /// </summary>
        /// <param name="stream">Binary Read-capable Stream of DATA0</param>
        /// <param name="leaveOpen">If true, won't dispose <paramref name="stream" /></param>
        public DATA0(Stream stream, bool leaveOpen = false)
        {
            try
            {
                if (!stream.CanRead) throw new InvalidOperationException("Cannot read from stream!");

                Span<byte> buffer = stackalloc byte[32];
                var entries = new DATA0Entry[stream.Length / 32];
                var i = 0;
                while (stream.Read(buffer) == 32) entries[i++] = MemoryMarshal.Read<DATA0Entry>(buffer);
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
        ///     lsof <seealso cref="DATA0Entry" />
        /// </summary>
        public List<DATA0Entry> Entries { get; }

        /// <summary>
        ///     Reads a file index from DATA1
        /// </summary>
        /// <param name="DATA1">Binary Read-capable Stream of DATA1</param>
        /// <param name="index">Entry Index to read</param>
        /// <returns>memory stream of uncompressed bytes</returns>
        public Memory<byte> ReadEntry(Stream DATA1, int index)
        {
            if (index < 0 || index >= Entries.Count) throw new IndexOutOfRangeException($"Index {index} does not exist!");

            return ReadEntry(DATA1, Entries[index]);
        }

        /// <summary>
        ///     Reads a file entry from DATA1
        /// </summary>
        /// <param name="DATA1">Binary Read-capable Stream of DATA1</param>
        /// <param name="entry">Entry to read</param>
        /// <returns>memory stream of uncompressed bytes</returns>
        public static Memory<byte> ReadEntry(Stream DATA1, DATA0Entry entry)
        {
            if (!DATA1.CanRead) throw new InvalidOperationException("Cannot read from stream!");
            if (entry.UncompressedSize == 0) return Memory<byte>.Empty;

            DATA1.Position = entry.Offset;

            if (entry.IsCompressed) return Decompress(DATA1, entry.CompressedSize);

            var buffer = new Memory<byte>(new byte[entry.UncompressedSize]);
            DATA1.Read(buffer.Span);
            return buffer;
        }

        public static Memory<byte> Decompress(Stream stream, long compressedSize)
        {
            var compressedBuffer = new Span<byte>(new byte[compressedSize + SizeHelper.SizeOf<CompressionInfo>()]);
            stream.Read(compressedBuffer);
            return Decompress(compressedBuffer);
        }

        public static unsafe Memory<byte> Decompress(Span<byte> data)
        {
            var cursor = 0;
            var compInfo = MemoryMarshal.Read<CompressionInfo>(data);
            var buffer = new Memory<byte>(new byte[compInfo.Size]);
            cursor += SizeHelper.SizeOf<CompressionInfo>();
            var chunkSizes = MemoryMarshal.Cast<byte, int>(data.Slice(cursor, 4 * compInfo.ChunkCount));
            cursor = (cursor + 4 * compInfo.ChunkCount).Align(0x80);
            var bufferCursor = 0;
            for (var i = 0; i < compInfo.ChunkCount; ++i)
            {
                var chunkSize = chunkSizes[i];
                try
                {
                    if (chunkSize + bufferCursor == buffer.Length)
                    {
                        data.Slice(cursor, chunkSize).CopyTo(buffer.Span.Slice(bufferCursor));
                        bufferCursor += chunkSize;
                        continue;
                    }

                    fixed (byte* pinData = &data.Slice(cursor)[6])
                    {
                        using var stream = new UnmanagedMemoryStream(pinData, chunkSize - 6);
                        using var deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
                        var block = new Span<byte>(new byte[0x0001_0000]);
                        var read = deflateStream.Read(block);
                        block.Slice(0, read).CopyTo(buffer.Span.Slice(bufferCursor));
                        bufferCursor = (bufferCursor + read).Align(0x80);
                    }
                }
                finally
                {
                    cursor = (cursor + chunkSize).Align(0x80);
                }
            }

            return buffer;
        }
    }
}
