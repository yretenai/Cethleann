using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann
{
    /// <summary>
    ///     Koei Engine Bundle, apparently because DataTable is too boring.
    /// </summary>
    [PublicAPI]
    public class Bundle
    {
        /// <summary>
        ///     Initialize with no data
        /// </summary>
        public Bundle()
        {
        }

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
        public List<Memory<byte>> Entries { get; set; } = new List<Memory<byte>>();

        /// <summary>
        ///     Writes bundle data
        /// </summary>
        /// <returns></returns>
        public Span<byte> Write()
        {
            var baseLength = (4 + 4 * Entries.Count).Align(0x10);
            var totalLength = baseLength + Entries.Sum(x => x.Length);
            var table = new Span<byte>(new byte[totalLength]);

            var count = Entries.Count;
            MemoryMarshal.Write(table, ref count);

            var dataOffset = baseLength;
            for (int i = 0; i < Entries.Count; ++i)
            {
                var record = Entries[i].Length;
                MemoryMarshal.Write(table.Slice(4 + 4 * i), ref record);
                Entries[i].Span.CopyTo(table.Slice(dataOffset));
                dataOffset += record;
            }

            return table;
        }

        /// <summary>
        ///     Checks if the stream makes sense to be a bundle
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[] Validate(Span<byte> data)
        {
            if (data.Length < 0x10) return null;
            var count = MemoryMarshal.Read<int>(data);
            if (count <= 0 || count > 0xFF) return null;

            var headerSize = count * 4;
            if (headerSize + 4 > data.Length || headerSize < 0) return null;
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
