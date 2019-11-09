using System;
using Cethleann.Structure.Resource;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    ///     F Section of G1M models
    /// </summary>
    public class G1MF : IG1Section
    {
        /// <summary>
        ///     Model F Data
        ///     Pay Respects.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        // ReSharper disable once UnusedParameter.Local
        public G1MF(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != ResourceSection.ModelF) throw new InvalidOperationException("Not an G1MF stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MF version {Section.Version.ToVersion()} is not supported!");
        }

        /// <inheritdoc />
        public int SupportedVersion { get; } = 29;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
    }
}
