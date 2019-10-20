using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Art;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// Geometry Section of G1M models
    /// </summary>
    public class G1MG : IG1Section
    {
        /// <inheritdoc/>
        public int SupportedVersion { get; } = 44;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }

        /// <summary>
        /// Model Geometry
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        public G1MG(Span<byte> data, bool ignoreVersion = false)
        {
            if (!data.Matches(DataType.ModelGeometry)) throw new InvalidOperationException("Not an G1MG stream");
            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MG version {Section.Version.ToVersion()} is not supported!");
        }
    }
}
