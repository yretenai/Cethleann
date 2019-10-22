using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;
using DragonLib;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Parses material data
    /// </summary>
    /// <inheritdoc />
    public class G1MGMaterial : IG1MGSection
    {
        /// <summary>
        ///     Material data found in this section
        /// </summary>
        public List<(ModelGeometryMaterial material, ModelGeometryTextureSet[] textureSet)> Materials = new List<(ModelGeometryMaterial, ModelGeometryTextureSet[])>();

        internal G1MGMaterial(Span<byte> data, ModelGeometrySection sectionInfo)
        {
            Section = sectionInfo;
            var offset = 0;
            for (var i = 0; i < sectionInfo.Count; ++i)
            {
                var material = MemoryMarshal.Read<ModelGeometryMaterial>(data.Slice(offset));
                offset += SizeHelper.SizeOf<ModelGeometryMaterial>();
                var bufferSize = SizeHelper.SizeOf<ModelGeometryTextureSet>() * material.Count;
                var textureSet = MemoryMarshal.Cast<byte, ModelGeometryTextureSet>(data.Slice(offset, bufferSize)).ToArray();
                offset += bufferSize;
                Materials.Add((material, textureSet));
            }
        }

        /// <inheritdoc />
        public ModelGeometrySection Section { get; }

        /// <inheritdoc />
        public ModelGeometryType Type => ModelGeometryType.Material;
    }
}
