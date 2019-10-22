using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;
using DragonLib;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     VertexBuffer Stride information
    /// </summary>
    public class G1MGVertexAttribute : IG1MGSection
    {
        internal G1MGVertexAttribute(Span<byte> data, ModelGeometrySection section)
        {
            Section = section;
            var offset = 0;
            BufferInfo = MemoryMarshal.Cast<byte, int>(data.Slice(offset, section.Count * 4)).ToArray();
            offset += offset * 4;
            var count = MemoryMarshal.Read<int>(data.Slice(offset));
            offset += 4;
            Attributes = MemoryMarshal.Cast<byte, ModelGeometryAttribute>(data.Slice(offset, count * SizeHelper.SizeOf<ModelGeometryAttribute>())).ToArray();
        }

        public int[] BufferInfo { get; }

        public ModelGeometryAttribute[] Attributes { get; }

        /// <inheritdoc />
        public ModelGeometryType Type => ModelGeometryType.VertexAttribute;

        /// <inheritdoc />
        public ModelGeometrySection Section { get; }
    }
}
