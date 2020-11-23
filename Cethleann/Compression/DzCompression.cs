using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using DragonLib;
using DragonLib.IO;

using JetBrains.Annotations;

namespace Cethleann.Compression
{
    /// <summary>
    ///     Compression helper class for TN's fork of KTGL
    /// </summary>
    [PublicAPI]
    public static class DzCompression
    {
        /// <summary>
        ///     Decompresses a .dz stream.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Span<byte> Decompress(Span<byte> data, CompressionOptions? options = null)
        {
            options ??= CompressionOptions.Default;
            var blockCount = MemoryMarshal.Read<int>(data);
            var ptr = 4;
            var decompressedSize = MemoryMarshal.Read<int>(data.Slice(ptr));
            ptr += 4;
            var sizes = MemoryMarshal.Cast<byte, int>(data.Slice(ptr, 4 * blockCount));
            ptr = (ptr + 4 * blockCount).Align(options.Alignment);
            var buffer = new Span<byte>(new byte[decompressedSize]);
            var decPtr = 0;
            for (var i = 0; i < blockCount; ++i)
            {
                var chunk = data.Slice(ptr, sizes[i]);
                ptr += sizes[i].Align(options.Alignment);
                if (i == blockCount - 1 && sizes[i] + decPtr == decompressedSize)
                {
                    chunk.CopyTo(buffer.Slice(decPtr));
                    break;
                }

                var blockSize = MemoryMarshal.Read<int>(chunk);
                if (blockSize > 0)
                {
                    using var stream = new MemoryStream(chunk.Slice(6, blockSize - 2).ToArray());
                    using var inflate = new DeflateStream(stream, CompressionMode.Decompress, true);
                    var decChunk = buffer.Slice(decPtr);
                    var r = inflate.Read(decChunk);
                    decPtr += r;
                }
                else
                {
                    chunk.Slice(4).CopyTo(buffer.Slice(decPtr));
                    decPtr += chunk.Length - 4;
                }
            }

            return buffer;
        }

        /// <summary>
        ///     Compresses a stream into a .gz stream.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Span<byte> Compress(Span<byte> data, CompressionOptions? options = null)
        {
            options ??= CompressionOptions.Default;
            var blocks    = (int) Math.Floor(data.Length / (float) options.BlockSize) + (data.Length % options.BlockSize == 0 ? 0 : 1);
            var dataStart = (4 + 4 * blocks).Align(options.Alignment);
            var header    = new Memory<byte>(new byte[dataStart]);
            var ptr       = 0;
            SpanHelper.WriteLittleInt(ref header, blocks, ref ptr);
            SpanHelper.WriteLittleInt(ref header, data.Length, ref ptr);
            var encPtr = 0;
            var buffer = new Memory<byte>(new byte[data.Length]);
            var bufPtr = 0;

            while (encPtr < data.Length)
            {
                var chunk = data.Slice(encPtr, Math.Min(options.BlockSize, data.Length - encPtr));
                encPtr += chunk.Length;

                var comBuffer = CompressionEncryption.CompressDEFLATEIonic(chunk, options.CompressionLevel);
                var size = comBuffer.Length + 2;

                if (chunk.Length < options.BlockSize && size >= chunk.Length && !options.ForceLastBlock)
                {
                    SpanHelper.WriteLittleInt(ref header, chunk.Length, ref ptr);
                    chunk.CopyTo(buffer.Slice(bufPtr).Span);
                    bufPtr = (bufPtr + chunk.Length).Align(options.Alignment);
                }
                else
                {
                    SpanHelper.WriteLittleInt(ref header, size + 4, ref ptr);
                    SpanHelper.WriteLittleInt(ref buffer, size, ref bufPtr);
                    SpanHelper.WriteLittleUShort(ref buffer, 0xDA78, ref bufPtr);
                    comBuffer.CopyTo(buffer.Slice(bufPtr).Span);
                    bufPtr = (bufPtr + comBuffer.Length).Align(options.Alignment);
                }
            }

            var totalBuffer = new Span<byte>(new byte[dataStart + bufPtr]);
            header.Span.CopyTo(totalBuffer);
            buffer.Span.Slice(0,  bufPtr).CopyTo(totalBuffer.Slice(dataStart));
            return totalBuffer;
        }
    }
}
