using Cethleann.G1.G1ModelSection;
using Cethleann.Structure.Art;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cethleann.G1
{
    public class G1Model : IG1Section
    {

        /// <inheritdoc/>
        public int SupportedVersion { get; } = 37;


        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }

        public List<IG1Section> Sections { get; } = new List<IG1Section>();

        public G1Model(Span<byte> data, bool ignoreVersion = false)
        {
            if (!data.Matches(DataType.Model)) throw new InvalidOperationException("Not an G1M stream");
            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1< version {Section.Version.ToVersion()} is not supported!");
        }
    }
}
