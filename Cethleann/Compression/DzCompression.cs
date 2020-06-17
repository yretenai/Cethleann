using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using DragonLib;
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
        /// <param name="align"></param>
        /// <returns></returns>
        public static Span<byte> Decompress(Span<byte> data, int align = 0x80)
        {
            var blockCount = MemoryMarshal.Read<int>(data);
            var ptr = 4;
            var decompressedSize = MemoryMarshal.Read<int>(data.Slice(ptr));
            ptr += 4;
            var sizes = MemoryMarshal.Cast<byte, int>(data.Slice(ptr, 4 * blockCount));
            ptr = (ptr + 4 * blockCount).Align(align);
            var buffer = new Span<byte>(new byte[decompressedSize]);
            var decPtr = 0;
            for (var i = 0; i < blockCount; ++i)
            {
                var chunk = data.Slice(ptr, sizes[i]);
                ptr += sizes[i].Align(align);
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
        /// <param name="blockSize"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public static Span<byte> Compress(Span<byte> data, int blockSize = 0x4000, int align = 0x80) => throw new NotImplementedException();
    }
}
