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
    ///     Decompress Standard KTGL streams
    /// </summary>
    [PublicAPI]
    public class Compression
    {
        /// <summary>
        ///     Decompresses a Gz stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="decompressedSize"></param>
        /// <param name="compressionFunc"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static Span<byte> Decompress(Span<byte> data, int decompressedSize, int compressionFunc, int blockSize = 0x4000)
        {
            unsafe
            {
                var decPtr = 0;
                Span<byte> decompressed = new byte[decompressedSize == -1 ? data.Length : decompressedSize];
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
                                Span<byte> block = new byte[blockSize * 4];
                                var decRead = inflate.Read(block);
                                if (decPtr + decRead > decompressed.Length)
                                {
                                    Span<byte> temp = new byte[decompressed.Length + data.Length + decRead];
                                    decompressed.CopyTo(temp);
                                    decompressed = temp;
                                }

                                block.Slice(0, decRead).CopyTo(decompressed.Slice(decPtr));
                                decPtr += decRead;
                            }

                            break;
                        }
                        case 2:
                        {
                            var chunk = data.Slice(comPtr, chunkSize);
                            Span<byte> block = new byte[blockSize * 4];
                            var decRead = CompressionEncryption.UnsafeDecompressLZ77EA_970(chunk, block);
                            if (decPtr + decRead > decompressed.Length)
                            {
                                Span<byte> temp = new byte[decPtr + decRead + blockSize * 4];
                                decompressed.CopyTo(temp);
                                decompressed = temp;
                            }

                            block.Slice(0, decRead).CopyTo(decompressed.Slice(decPtr));
                            decPtr += decRead;
                            break;
                        }
                    }

                    comPtr += chunkSize;
                }

                Logger.Assert(comPtr == data.Length, "comPtr == data.Length");
                return decompressed.Slice(0, decPtr);
            }
        }
    }
}
