using System;
using System.Runtime.InteropServices;
using Cethleann.Koei.Structure.Resource;
using DragonLib.Numerics;
using JetBrains.Annotations;

namespace Cethleann.Koei.G1.G1ModelSection
{
    /// <summary>
    ///     Matrix Section of G1M models
    /// </summary>
    [PublicAPI]
    public class G1MM : IG1Section
    {
        /// <summary>
        ///     Model Matrix Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        public G1MM(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != ResourceSection.ModelMatrix) throw new InvalidOperationException("Not an G1MM stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MM version {Section.Version.ToVersion()} is not supported!");

            var _ = MemoryMarshal.Read<int>(data.Slice(0xC)); // count
            Matrices = MemoryMarshal.Cast<byte, Matrix4x4>(data.Slice(0x10)).ToArray();
        }

        /// <summary>
        ///     List of matrices found in the file.
        ///     They're all weird.
        /// </summary>
        public Matrix4x4[] Matrices { get; }

        /// <inheritdoc />
        public int SupportedVersion { get; } = 20;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
    }
}
