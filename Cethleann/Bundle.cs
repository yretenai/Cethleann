using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
            var offset = 0x30;
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
            if (MemoryMarshal.Read<int>(data) != 9) return null;

            var sizes = MemoryMarshal.Cast<byte, int>(data.Slice(4, 4 * 11)).ToArray();
            return sizes.Sum() + 0x30 != data.Length ? null : sizes;
        }
    }
}
