using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.G1.G1ModelSection.G1MGSection;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Model;
using DragonLib;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    ///     Geometry Section of G1M models
    /// </summary>
    public class G1MG : IG1Section
    {
        /// <summary>
        ///     Model Geometry
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        public G1MG(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != DataType.ModelGeometry) throw new InvalidOperationException("Not an G1MG stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MG version {Section.Version.ToVersion()} is not supported!");

            Header = MemoryMarshal.Read<ModelGeometryHeader>(data);
            Helper.Assert(Header.ModelType.ToFourCC(true) == "NX_", "ModelType == NX_");

            var offset = SizeHelper.SizeOf<ModelGeometryHeader>();
            while (offset < data.Length)
            {
                var subSectionHeader = MemoryMarshal.Read<ModelGeometrySection>(data.Slice(offset));
                var block = data.Slice(offset + SizeHelper.SizeOf<ModelGeometrySection>(), subSectionHeader.Size - SizeHelper.SizeOf<ModelGeometrySection>());
                offset += subSectionHeader.Size;
                var section = subSectionHeader.Magic switch
                {
                    ModelGeometryType.Lattice => (IG1MGSection) new G1MGLattice(block, subSectionHeader),
                    ModelGeometryType.Material => new G1MGMaterial(block, subSectionHeader),
                    ModelGeometryType.ShaderParam => new G1MGShaderParam(block, subSectionHeader),
                    _ => throw new NotSupportedException($"Can't handle G1MG section {subSectionHeader.Magic:F}")
                };
                SubSections.Add(section);
            }
        }

        public ModelGeometryHeader Header { get; }

        public List<IG1MGSection> SubSections { get; set; } = new List<IG1MGSection>();

        /// <inheritdoc />
        public int SupportedVersion { get; } = 44;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
    }
}
