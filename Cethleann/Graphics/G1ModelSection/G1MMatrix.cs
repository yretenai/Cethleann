using System;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using Cethleann.Structure.Resource;
using DragonLib.Numerics;
using JetBrains.Annotations;

namespace Cethleann.Graphics.G1ModelSection
{
    /// <summary>
    ///     Matrix Section of G1M models
    /// </summary>
    [PublicAPI]
    public class G1MMatrix : IKTGLSection
    {
        /// <summary>
        ///     Model Matrix Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        public G1MMatrix(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != DataType.G1MM) throw new InvalidOperationException("Not an G1MM stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MM version {Section.Version.ToVersion()} is not supported!");

            if (Section.Size == 0xC) return;

            var count = MemoryMarshal.Read<int>(data);
            if (count == 0) return;
            Matrices = MemoryMarshal.Cast<byte, Matrix4x4>(data.Slice(0x4)).ToArray();
        }

        /// <summary>
        ///     List of matrices found in the file.
        ///     They're all weird.
        /// </summary>
        public Matrix4x4[] Matrices { get; } = new Matrix4x4[0];

        /// <inheritdoc />
        public int SupportedVersion { get; } = 20;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
    }
}
