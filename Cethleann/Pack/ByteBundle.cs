using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Cethleann.Pack
{
    /// <summary>
    ///     Koei Engine Byte Bundle, apparently because Pointer Bundle is too boring.
    /// </summary>
    [PublicAPI]
    public class ByteBundle
    {
        /// <summary>
        ///     Initialize with no data
        /// </summary>
        public ByteBundle()
        {
        }

        /// <summary>
        ///     Split file into individual chunks
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="InvalidDataException"></exception>
        public ByteBundle(Span<byte> data)
        {
            var pointers = Validate(data);
            if (pointers == null) throw new InvalidDataException("Not a valid bundle stream.");
            for (var index = 0; index < pointers.Length - 1; index++)
            {
                var pointer = pointers[index];
                var size = data.Length - pointer;
                if (index + 1 < pointers.Length) size = pointers[index + 1] - pointer;
                Entries.Add(new Memory<byte>(data.Slice(pointer, size).ToArray()));
            }
        }

        /// <summary>
        ///     lsof data blobs
        /// </summary>
        public List<Memory<byte>> Entries { get; set; } = new List<Memory<byte>>();

        /// <summary>
        ///     Writes bundle data
        /// </summary>
        /// <returns></returns>
        public Span<byte> Write()
        {
            var count = Entries.Count;
            var baseLength = 1 + 4 * count;
            if (count >= 0xFF) baseLength += 2;
            if (count >= 0xFFFF) baseLength += 4;
            var totalLength = baseLength + Entries.Sum(x => x.Length);
            var table = new Span<byte>(new byte[totalLength]);

            var ptr = 1;
            if (count < 0xFF)
            {
                table[0] = (byte) count;
            }
            else
            {
                if (count < 0xFFFF)
                {
                    table[0] = 0xFF;
                    var tmp = (short) count;
                    MemoryMarshal.Write(table.Slice(ptr), ref tmp);
                    ptr += 2;
                }
                else
                {
                    table[1] = 0xFF;
                    table[2] = 0xFF;
                    ptr += 2;
                    MemoryMarshal.Write(table.Slice(ptr), ref count);
                    ptr += 4;
                }
            }

            var dataOffset = baseLength;
            for (var i = 0; i < Entries.Count; ++i)
            {
                MemoryMarshal.Write(table.Slice(ptr + 4 * i), ref dataOffset);
                Entries[i].Span.CopyTo(table.Slice(dataOffset));
                dataOffset += Entries[i].Length;
            }

            return table;
        }

        /// <summary>
        ///     Checks if the stream makes sense to be a byte bundle
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[]? Validate(Span<byte> data)
        {
            if (data.Length < 0x9) return null;
            var count = (int) data[0];
            var ptr = 1;
            if (count == 0xFF)
            {
                count = MemoryMarshal.Read<short>(data.Slice(ptr));
                ptr += 2;
                if (count == -1)
                {
                    count = MemoryMarshal.Read<int>(data.Slice(ptr));
                    ptr += 4;
                    // The asmcode enters a weird catch-22 where it checks if int is 0xFFFFFFFF and then crashes.
                    if (count == -1) return null;
                }
            }

            count += 1;

            var headerSize = count * 4;
            if (headerSize + ptr > data.Length || headerSize < 0) return null;
            var pointers = MemoryMarshal.Cast<byte, int>(data.Slice(ptr, headerSize)).ToArray();
            try
            {
                var l = data.Length;
                return pointers[0] == headerSize + ptr && pointers.All(x => x <= l) && pointers[count - 1] == l ? pointers : null;
            }
            catch (ArithmeticException)
            {
                return null;
            }
        }
    }
}
