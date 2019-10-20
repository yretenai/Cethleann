using Cethleann.Structure.Art;
using System;
using DragonLib;
using System.Runtime.InteropServices;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// Matrix Section of G1M models
    /// </summary>
    public class G1MM : IG1Section
    {
        /// <inheritdoc/>
        public int SupportedVersion { get; } = 20;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }

        /// <summary>
        /// List of matrices found in the file. 
        /// They're all weird.
        /// </summary>
        public Matrix4x4[] Matrices { get; }

        /// <summary>
        /// Model Matrix Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        public G1MM(Span<byte> data, bool ignoreVersion = false)
        {
            if (!data.Matches(DataType.ModelMatrix)) throw new InvalidOperationException("Not an G1MM stream");
            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MM version {Section.Version.ToVersion()} is not supported!");
            var count = MemoryMarshal.Read<int>(data.Slice(0xC));
            Matrices = MemoryMarshal.Cast<byte, Matrix4x4>(data.Slice(0x10)).ToArray();
        }
    }
}
