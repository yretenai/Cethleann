using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DragonLib;

namespace Cethleann
{
    /// <summary>
    ///     Koei Engine Bundle, apparently because DataTable is too boring.
    /// </summary>
    public class Bundle
    {
        /// <summary>
        ///     Split file into individual chunks
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="InvalidDataException"></exception>
        public Bundle(Span<byte> data)
        {
            var sizes = Validate(data);
            if (sizes == null) throw new InvalidDataException("Not a valid bundle stream.");
            var offset = (4 + sizes.Length * 4).Align(0x10);
            foreach (var size in sizes)
            {
                Entries.Add(new Memory<byte>(data.Slice(offset, size).ToArray()));
                offset += size;
            }
        }

        /// <summary>
        ///     lsof data blobs
        /// </summary>
        public List<Memory<byte>> Entries { get; } = new List<Memory<byte>>();

        /// <summary>
        ///     Checks if the stream makes sense to be a bundle
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[] Validate(Span<byte> data)
        {
            var count = MemoryMarshal.Read<int>(data);
            if (count < 0 || count > 0xFF) return null;

            var headerSize = count * 4;
            if (headerSize > data.Length || headerSize < 0) return null;
            var sizes = MemoryMarshal.Cast<byte, int>(data.Slice(4, headerSize)).ToArray();
            try
            {
                return sizes.Sum() + (headerSize + 4).Align(0x10) != data.Length ? null : sizes;
            }
            catch (ArithmeticException)
            {
                return null;
            }
        }
    }
}
