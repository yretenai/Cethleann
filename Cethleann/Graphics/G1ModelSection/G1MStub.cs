using System;
using Cethleann.Structure.Resource;
using JetBrains.Annotations;

namespace Cethleann.Graphics.G1ModelSection
{
    /// <summary>
    ///     Stub G1M Section
    /// </summary>
    [PublicAPI]
    public class G1MStub : IKTGLSection
    {
        /// <inheritdoc />
        public int SupportedVersion { get; } = int.MaxValue;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
        
        /// <summary>
        ///     Memory Buffer for this section
        /// </summary>
        public Memory<byte> Data { get; }

        /// <summary>
        ///     Initialize with memory block
        /// </summary>
        /// <param name="dataBlock"></param>
        /// <param name="sectionHeader"></param>
        public G1MStub(Span<byte> dataBlock, ResourceSectionHeader sectionHeader)
        {
            Data = new Memory<byte>(dataBlock.ToArray());
            Section = sectionHeader;
        }
    }
}
