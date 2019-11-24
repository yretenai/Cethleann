using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;

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
    }
}
