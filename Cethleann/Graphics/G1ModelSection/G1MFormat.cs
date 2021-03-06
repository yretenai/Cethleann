﻿using System;
using Cethleann.Structure;
using Cethleann.Structure.Resource;
using JetBrains.Annotations;

namespace Cethleann.Graphics.G1ModelSection
{
    /// <summary>
    ///     F Section of G1M models
    /// </summary>
    [PublicAPI]
    public class G1MFormat : IKTGLSection
    {
        /// <summary>
        ///     Model format Data
        ///     Pay Respects.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        public G1MFormat(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != DataType.G1MF) throw new InvalidOperationException("Not an G1MF stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MF version {Section.Version.ToVersion()} is not supported!");
        }

        /// <inheritdoc />
        public int SupportedVersion { get; } = 29;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
    }
}
