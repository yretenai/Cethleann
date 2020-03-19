using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Pack
{
    /// <summary>
    ///     Koei Engine Pointer Bundle, apparently because Bundle is too boring.
    /// </summary>
    [PublicAPI]
    public class PointerBundle
    {
        /// <summary>
        ///     Initialize with no data
        /// </summary>
        public PointerBundle()
        {
        }

        /// <summary>
        ///     Split file into individual chunks
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="InvalidDataException"></exception>
        public PointerBundle(Span<byte> data)
        {
            var pointers = Validate(data);
            if (pointers == null) throw new InvalidDataException("Not a valid bundle stream.");
            for (var index = 0; index < pointers.Length; index++)
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
            var baseLength = (4 + 4 * Entries.Count).Align(0x10);
            var totalLength = baseLength + Entries.Sum(x => x.Length);
            var table = new Span<byte>(new byte[totalLength]);

            var count = Entries.Count;
            MemoryMarshal.Write(table, ref count);

            var dataOffset = baseLength;
            for (int i = 0; i < Entries.Count; ++i)
            {
                MemoryMarshal.Write(table.Slice(4 + 4 * i), ref dataOffset);
                Entries[i].Span.CopyTo(table.Slice(dataOffset));
                dataOffset += Entries[i].Length;
            }

            return table;
        }

        /// <summary>
        ///     Checks if the stream makes sense to be a pointer bundle
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[]? Validate(Span<byte> data)
        {
            if (data.Length < 0x10) return null;
            var count = MemoryMarshal.Read<int>(data);
            if (count <= 0 || count > 0xFF) return null;

            var headerSize = count * 4;
            if (headerSize + 4 > data.Length || headerSize < 0) return null;
            var pointers = MemoryMarshal.Cast<byte, int>(data.Slice(4, headerSize)).ToArray();
            try
            {
                var l = data.Length;
                return pointers[0] == (headerSize + 4).Align(0x10) && pointers.All(x => x < l) ? pointers : null;
            }
            catch (ArithmeticException)
            {
                return null;
            }
        }
    }
}
