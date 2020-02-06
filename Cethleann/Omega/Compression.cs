using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Omega
{
    /// <summary>
    ///     (De)compress Omega streams
    /// </summary>
    [PublicAPI]
    public class Compression
    {
        /// <summary>
        ///     Decompresses a Gz stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="decompressedSize"></param>
        /// <returns></returns>
        public static Span<byte> Decompress(Span<byte> data, int decompressedSize)
        {
            unsafe
            {
                var decPtr = 0;
                Span<byte> decompressed = new byte[decompressedSize];
                var comPtr = 0;

                var size = BinaryPrimitives.ReadInt32LittleEndian(data);
                comPtr += 4;
                Logger.Assert(size == decompressedSize, "size == decompressedSize");
                while (true)
                {
                    if (comPtr >= data.Length) break;
                    var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(comPtr));
                    comPtr += 4;
                    if (chunkSize == 0) break;

                    var chunk = data.Slice(comPtr + 2, chunkSize - 2);
                    fixed (byte* pin = &chunk.GetPinnableReference())
                    {
                        using var stream = new UnmanagedMemoryStream(pin, chunk.Length);
                        Logger.Assert(data[comPtr] == 0x78, "data[comPtr] == 0x78");
                        Logger.Assert(data[comPtr + 1] == 0xDA, "data[comPtr + 1] == 0xDA");
                        using var inflate = new DeflateStream(stream, CompressionMode.Decompress, true);
                        var decChunk = decompressed.Slice(decPtr);
                        decPtr += inflate.Read(decChunk);
                    }

                    comPtr += chunkSize;
                }

                Logger.Assert(comPtr == data.Length, "comPtr == data.Length");
                return decompressed;
            }
        }
    }
}
