using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Koei.Structure.Resource.Model;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Koei.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     VertexBuffer Stride information
    /// </summary>
    [PublicAPI]
    public class G1MGVertexAttribute : IG1MGSection
    {
        internal G1MGVertexAttribute(Span<byte> data, ModelGeometrySection section)
        {
            Section = section;
            var offset = 0;
            for (var i = 0; i < section.Count; ++i)
            {
                var count = MemoryMarshal.Read<int>(data.Slice(offset));
                offset += 4;
                var index = MemoryMarshal.Cast<byte, int>(data.Slice(offset, count * 4)).ToArray();
                offset += count * 4;
                count = MemoryMarshal.Read<int>(data.Slice(offset));
                offset += 4;
                var attributes = MemoryMarshal.Cast<byte, ModelGeometryAttribute>(data.Slice(offset, count * SizeHelper.SizeOf<ModelGeometryAttribute>())).ToArray();
                offset += count * SizeHelper.SizeOf<ModelGeometryAttribute>();
                Attributes.Add((index, attributes));
            }
        }

        /// <summary>
        ///     Describes vertex strides
        /// </summary>
        public List<(int[] index, ModelGeometryAttribute[] attributeList)> Attributes { get; } = new List<(int[] sizeMap, ModelGeometryAttribute[] attributeList)>();

        /// <inheritdoc />
        public ModelGeometryType Type => ModelGeometryType.VertexAttribute;

        /// <inheritdoc />
        public ModelGeometrySection Section { get; }
    }
}
