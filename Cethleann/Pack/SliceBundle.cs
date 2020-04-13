using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.Pack;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Pack
{
    /// <summary>
    ///     Koei Engine Slice Bundle, apparently because DataTable is too boring.
    /// </summary>
    [PublicAPI]
    public class SliceBundle
    {
        /// <summary>
        ///     Split file into individual chunks
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="InvalidDataException"></exception>
        public SliceBundle(Span<byte> data)
        {
            var sizes = Validate(data);
            if (sizes == null) throw new InvalidDataException("Not a valid bundle stream.");
            foreach (var (pointer, size) in sizes) Entries.Add(new Memory<byte>(data.Slice(pointer, size).ToArray()));
        }

        /// <summary>
        ///     lsof data blobs
        /// </summary>
        public List<Memory<byte>> Entries { get; set; } = new List<Memory<byte>>();

        /// <summary>
        ///     Checks if the stream makes sense to be a bundle
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static (int pointer, int size)[]? Validate(Span<byte> data)
        {
            if (data.Length < 0x110) return null;
            var header = MemoryMarshal.Read<SliceBundleHeader>(data);
            if (header.Count < 1 || header.Count > 32) return null;

            var pointers = MemoryMarshal.Cast<byte, int>(data.Slice(SizeHelper.SizeOf<SliceBundleHeader>().Align(0x10), header.Count * 4)).ToArray();
            var length = data.Length;
            if (pointers.Any(x => x > length || x < 0x110)) return null;
            var sizes = MemoryMarshal.Cast<byte, int>(data.Slice(SizeHelper.SizeOf<SliceBundleHeader>().Align(0x10) + 0x80, header.Count * 4)).ToArray();

            try
            {
                return sizes.Select(x => x.Align(0x10)).Sum() + 0x110 != data.Length ? null : sizes.Select((size, index) => (pointers[index], size)).ToArray();
            }
            catch (ArithmeticException)
            {
                return null;
            }
        }
    }
}
