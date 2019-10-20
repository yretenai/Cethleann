using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Art;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// Extra Section of G1M models
    /// </summary>
    public class G1MExtra : IG1Section
    {
        /// <inheritdoc/>
        public int SupportedVersion { get; } = 10;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }

        /// <summary>
        /// Extra data found in G1M models.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        public G1MExtra(Span<byte> data, bool ignoreVersion = false)
        {
            if (!data.Matches(DataType.ModelExtra)) throw new InvalidOperationException("Not an EXTR stream");
            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"EXTR version {Section.Version.ToVersion()} is not supported!");
        }
    }
}
