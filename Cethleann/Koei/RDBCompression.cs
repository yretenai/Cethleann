using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Koei
{
    /// <summary>
    ///     (De)compress Omega streams
    /// </summary>
    [PublicAPI]
    public class RDBCompression
    {
        /// <summary>
        ///     Decompresses a Gz stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="decompressedSize"></param>
        /// <param name="compressionFunc"></param>
        /// <returns></returns>
        public static Span<byte> Decompress(Span<byte> data, int decompressedSize, int compressionFunc)
        {
            unsafe
            {
                var decPtr = 0;
                Span<byte> decompressed = new byte[decompressedSize];
                var comPtr = 0;
                while (true)
                {
                    if (comPtr >= data.Length) break;
                    var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(comPtr));
                    comPtr += 4;
                    if (chunkSize == 0) break;

                    switch (compressionFunc)
                    {
                        case 1:
                        {
                            var chunk = data.Slice(comPtr + 2, chunkSize - 2);
                            fixed (byte* pin = &chunk.GetPinnableReference())
                            {
                                using var stream = new UnmanagedMemoryStream(pin, chunk.Length);
                                Logger.Assert(data[comPtr] == 0x78, "data[comPtr] == 0x78");
                                using var inflate = new DeflateStream(stream, CompressionMode.Decompress, true);
                                var decChunk = decompressed.Slice(decPtr);
                                decPtr += inflate.Read(decChunk);
                            }

                            break;
                        }
                        case 2:
                        {
                            var chunk = data.Slice(comPtr, chunkSize);
                            var decChunk = decompressed.Slice(decPtr);
                            decPtr += CompressionEncryption.UnsafeDecompressLZ77EA_970(chunk, decChunk);
                            break;
                        }
                    }

                    comPtr += chunkSize;
                }

                Logger.Assert(comPtr == data.Length, "comPtr == data.Length");
                return decompressed;
            }
        }
    }
}
