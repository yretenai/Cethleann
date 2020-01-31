using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using Cethleann.Structure.Resource;
using DragonLib;

namespace Cethleann.G1
{
    /// <summary>
    ///     Generic reader for KTGLSections
    /// </summary>
    public class PackedResource
    {
        /// <summary>
        ///     Initialize with no data
        /// </summary>
        public PackedResource()
        {
        }

        /// <summary>
        ///     Initialize with a count
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        public PackedResource(Span<byte> data, int count)
        {
            var offset = 0;
            for (int i = 0; i < count; ++i)
            {
                var header = MemoryMarshal.Read<ResourceSectionHeader>(data.Slice(offset));
                var blob = data.Slice(offset, header.Size);
                Sections.Add(new Memory<byte>(blob.ToArray()));
                offset += header.Size;
            }
        }

        /// <summary>
        ///     lsof sections in the file.
        /// </summary>
        public List<Memory<byte>> Sections { get; set; } = new List<Memory<byte>>();

        /// <summary>
        ///     Create a file with the given parameters
        /// </summary>
        /// <param name="header"></param>
        /// <param name="target"></param>
        /// <param name="version"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Span<byte> WriteWithHeader<T>(T header, DataType target, int version) where T : struct
        {
            var length = SizeHelper.SizeOf<T>() + Sections.Sum(x => x.Length) + SizeHelper.SizeOf<ResourceSectionHeader>();
            var pointer = 0;
            var buffer = new Span<byte>(new byte[length]);
            var resourceHeader = new ResourceSectionHeader
            {
                Magic = target,
                Size = length,
                Version = version.ToVersionA()
            };
            MemoryMarshal.Write(buffer, ref resourceHeader);
            pointer += SizeHelper.SizeOf<ResourceSectionHeader>();
            MemoryMarshal.Write(buffer.Slice(pointer), ref header);
            pointer += SizeHelper.SizeOf<T>();
            foreach (var stream in Sections)
            {
                stream.Span.CopyTo(buffer.Slice(pointer));
                pointer += stream.Length;
            }

            return buffer;
        }
    }
}
