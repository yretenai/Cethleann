using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.G1.G1ModelSection;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Model;
using DragonLib;

namespace Cethleann.G1
{
    /// <summary>
    ///     G1Model is the main model format
    /// </summary>
    public class G1Model : IG1Section
    {
        /// <summary>
        ///     Parse G1M from the provided data buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        public G1Model(Span<byte> data, bool ignoreVersion = false)
        {
            if (!data.Matches(DataType.Model)) throw new InvalidOperationException("Not an G1M stream");

            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1M version {Section.Version.ToVersion()} is not supported!");

            var header = MemoryMarshal.Read<ModelHeader>(data.Slice(0xC));
            var offset = header.HeaderSize;
            for (var i = 0; i < header.SectionCount; ++i)
            {
                var sectionHeader = MemoryMarshal.Read<ResourceSectionHeader>(data.Slice(offset));
                var block = data.Slice(offset + SizeHelper.SizeOf<ResourceSectionHeader>(), sectionHeader.Size - SizeHelper.SizeOf<ResourceSectionHeader>());
                var section = sectionHeader.Magic switch
                {
                    DataType.ModelSkeleton => (IG1Section) new G1MS(block, ignoreVersion, sectionHeader),
                    DataType.ModelF => new G1MF(block, ignoreVersion, sectionHeader),
                    DataType.ModelGeometry => new G1MG(block, ignoreVersion, sectionHeader),
                    DataType.ModelMatrix => new G1MM(block, ignoreVersion, sectionHeader),
                    DataType.ModelExtra => new G1MExtra(block, ignoreVersion, sectionHeader),
                    _ => throw new NotImplementedException($"Section {sectionHeader.Magic.ToFourCC(false)} not supported!")
                };
                Sections.Add(section);

                offset += sectionHeader.Size;
            }
        }

        /// <summary>
        ///     Sections found in this model.
        ///     Look for <seealso cref="G1MG" /> for Geometry.
        ///     Look for <seealso cref="G1MS" /> for Skeleton.
        /// </summary>
        public List<IG1Section> Sections { get; } = new List<IG1Section>();

        /// <inheritdoc />
        public int SupportedVersion { get; } = 37;


        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
    }
}
