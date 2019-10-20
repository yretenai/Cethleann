using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Art;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// F Section of G1M models
    /// </summary>
    public class G1MF : IG1Section
    {
        /// <inheritdoc/>
        public int SupportedVersion { get; } = 29;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }

        /// <summary>
        /// Model F Data
        /// Pay Respects.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        public G1MF(Span<byte> data, bool ignoreVersion = false)
        {
            if (!data.Matches(DataType.ModelF)) throw new InvalidOperationException("Not an G1MF stream");
            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MF version {Section.Version.ToVersion()} is not supported!");
        }
    }
}
