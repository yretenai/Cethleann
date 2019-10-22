using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;
using DragonLib;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Parses mesh bones
    /// </summary>
    public class G1MGBone : IG1MGSection
    {
        internal G1MGBone(Span<byte> data, ModelGeometrySection section)
        {
            Section = section;

            var offset = 0;
            for (int i = 0; i < section.Count; ++i)
            {
                var count = MemoryMarshal.Read<int>(data.Slice(offset));
                offset += 4;
                Bones.Add(MemoryMarshal.Cast<byte, ModelGeometryBone>(data.Slice(offset, count * SizeHelper.SizeOf<ModelGeometryBone>())).ToArray());
                offset += count * SizeHelper.SizeOf<ModelGeometryBone>();
            }
        }

        /// <summary>
        ///     Bone map
        /// </summary>
        public List<ModelGeometryBone[]> Bones { get; } = new List<ModelGeometryBone[]>();

        /// <inheritdoc />
        public ModelGeometryType Type => ModelGeometryType.Bone;

        /// <inheritdoc />
        public ModelGeometrySection Section { get; }
    }
}
