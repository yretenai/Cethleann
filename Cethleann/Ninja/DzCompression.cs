using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Ninja
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
                var blockSize = MemoryMarshal.Read<int>(chunk);
                var compressedChunk = chunk.Slice(6, blockSize - 2);
                ptr += sizes[i].Align(align);
                unsafe
                {
                    fixed (byte* pin = &compressedChunk.GetPinnableReference())
                    {
                        using var stream = new UnmanagedMemoryStream(pin, data.Length);
                        using var inflate = new DeflateStream(stream, CompressionMode.Decompress, true);
                        var decChunk = buffer.Slice(decPtr);
                        var r = inflate.Read(decChunk);;
                        decPtr += r;
                    }
                }
            }

            return buffer;
        }

        /// <summary>
        ///     Compresses a stream into a .gz stream.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static Span<byte> Compress(Span<byte> data, int blockSize = 0x4000)
        {
            return Span<byte>.Empty;
        }
    }
}
