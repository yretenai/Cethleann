using System;
using Cethleann.Structure.Resource;
using JetBrains.Annotations;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    ///     Extra Section of G1M models
    /// </summary>
    [PublicAPI]
    public class IG1Extra : IKTGLSection
    {
        /// <summary>
        ///     Extra data found in G1M models.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        public IG1Extra(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != DataType.ModelExtra) throw new InvalidOperationException("Not an EXTR stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"EXTR version {Section.Version.ToVersion()} is not supported!");
        }

        /// <inheritdoc />
        public int SupportedVersion { get; } = 10;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
    }
}
