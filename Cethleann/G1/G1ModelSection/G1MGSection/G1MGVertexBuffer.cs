using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;
using DragonLib;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Copies raw vertex buffers.
    /// </summary>
    public class G1MGVertexBuffer : IG1MGSection
    {
        internal G1MGVertexBuffer(Span<byte> block, ModelGeometrySection subSectionHeader)
        {
            Section = subSectionHeader;

            var offset = 0;
            for (int i = 0; i < subSectionHeader.Count; ++i)
            {
                var info = MemoryMarshal.Read<ModelGeometryVertexBuffer>(block.Slice(offset));
                offset += SizeHelper.SizeOf<ModelGeometryVertexBuffer>();
                var memory = new Memory<byte>(block.Slice(offset, info.Stride * info.Count).ToArray());
                offset += memory.Length;
                Buffers.Add((info, memory));
            }
        }

        /// <summary>
        ///     List of vertex buffer strides
        /// </summary>
        public List<(ModelGeometryVertexBuffer info, Memory<byte> buffer)> Buffers { get; set; } = new List<(ModelGeometryVertexBuffer info, Memory<byte> buffer)>();

        /// <inheritdoc />
        public ModelGeometryType Type => ModelGeometryType.VertexBuffer;

        /// <inheritdoc />
        public ModelGeometrySection Section { get; }
    }
}
