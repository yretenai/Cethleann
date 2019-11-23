using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Cethleann.Gust
{
    /// <summary>
    ///     Compression helper class for Gust's fork of KTGL
    /// </summary>
    [PublicAPI]
    public static class Compression
    {
        /// <summary>
        ///     Compresses a stream into a .gz stream.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static Span<byte> Compress(Span<byte> data, int blockSize = 0x4000)
        {
            var buffer = new Span<byte>(new byte[data.Length + (data.Length / blockSize) * 4]);
            var bufferCursor = 0;
            int dataCursor;
            for (dataCursor = 0; dataCursor <= data.Length; dataCursor += blockSize)
            {
                using var ms = new MemoryStream(blockSize);
                using var deflateStream = new DeflateStream(ms, CompressionLevel.Optimal);
                deflateStream.Write(data.Slice(dataCursor));
                deflateStream.Flush();
                var write = ms.Position + 2;
                var block = new Span<byte>(new byte[ms.Length]);
                ms.Position = 0;
                ms.Read(block);
                MemoryMarshal.Write(buffer.Slice(bufferCursor), ref write);
                bufferCursor += 6;
                buffer[bufferCursor - 2] = 0x78;
                buffer[bufferCursor - 1] = 0xDA;
                block.CopyTo(buffer.Slice(bufferCursor));
                bufferCursor += block.Length;
            }

            int zero = 0;
            MemoryMarshal.Write(data.Slice(dataCursor), ref zero);
            return buffer.Slice(0, dataCursor + 4);
        }

        /// <summary>
        ///     Decompresses a .gz stream.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static unsafe Span<byte> Decompress(Span<byte> data, int blockSize = 0x4000)
        {
            var buffer = new Span<byte>(new byte[data.Length * 2]);
            var bufferCursor = 0;
            var dataCursor = 0;
            while (true)
            {
                var size = MemoryMarshal.Read<int>(data.Slice(dataCursor));
                if (size == 0) break;
                dataCursor += 4;
                fixed (byte* pinData = &data.Slice(dataCursor)[2])
                {
                    using var stream = new UnmanagedMemoryStream(pinData, size - 2);
                    using var inflateStream = new DeflateStream(stream, CompressionMode.Decompress);
                    var block = new Span<byte>(new byte[blockSize]);
                    var read = inflateStream.Read(block);
                    if (buffer.Length < bufferCursor + read)
                    {
                        var tmp = new Span<byte>(new byte[buffer.Length * 2]);
                        buffer.CopyTo(tmp);
                        buffer = tmp;
                    }

                    block.Slice(0, read).CopyTo(buffer.Slice(bufferCursor));
                    bufferCursor += read;
                }

                dataCursor += size;
            }

            return buffer;
        }
    }
}
