using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Graphics.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Parses mesh bones
    /// </summary>
    [PublicAPI]
    public class G1MGBone : IG1MGSection
    {
        internal G1MGBone(Span<byte> data, ModelSection section)
        {
            Section = section;

            var offset = 0;
            for (var i = 0; i < section.Count; ++i)
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
        public ModelGeometrySectionType Type => ModelGeometrySectionType.Bone;

        /// <inheritdoc />
        public ModelSection Section { get; }
    }
}
