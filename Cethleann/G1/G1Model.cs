using Cethleann.G1.G1ModelSection;
using Cethleann.Structure.Art;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cethleann.G1
{
    /// <summary>
    /// G1Model is the main model format
    /// </summary>
    public class G1Model : IG1Section
    {

        /// <inheritdoc/>
        public int SupportedVersion { get; } = 37;


        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }

        /// <summary>
        /// Sections found in this model.
        /// Look for <seealso cref="G1MG"/> for Geometry.
        /// Look for <seealso cref="G1MS"/> for Skeleton.
        /// </summary>
        public List<IG1Section> Sections { get; } = new List<IG1Section>();

        /// <summary>
        /// Parse G1M from the provided data buffer
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
            for (int i = 0; i < header.SectionCount; ++i)
            {
                var section = MemoryMarshal.Read<ResourceSectionHeader>(data.Slice(offset));
                switch (section.Magic)
                {
                    case DataType.ModelSkeleton:
                        Sections.Add(new G1MS(data.Slice(offset, section.Size), ignoreVersion));
                        break;
                    case DataType.ModelF:
                        Sections.Add(new G1MF(data.Slice(offset, section.Size), ignoreVersion));
                        break;
                    case DataType.ModelGeometry:
                        Sections.Add(new G1MG(data.Slice(offset, section.Size), ignoreVersion));
                        break;
                    case DataType.ModelMatrix:
                        Sections.Add(new G1MM(data.Slice(offset, section.Size), ignoreVersion));
                        break;
                    case DataType.ModelExtra:
                        Sections.Add(new G1MExtra(data.Slice(offset, section.Size), ignoreVersion));
                        break;
                    default:
                        throw new NotImplementedException($"Section {section.Magic.ToFourCC()} not supported!");
                }
                offset += section.Size;
            }
        }
    }
}
