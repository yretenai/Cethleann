using System;
using Cethleann.Structure.Resource;
using JetBrains.Annotations;

namespace Cethleann.Graphics.G1ModelSection
{
    [PublicAPI]
    public class G1MStub : IKTGLSection
    {
        public int SupportedVersion { get; } = int.MaxValue;
        public ResourceSectionHeader Section { get; }
        public Memory<byte> Data { get; }

        public G1MStub(Span<byte> dataBlock, ResourceSectionHeader sectionHeader)
        {
            Data = new Memory<byte>(dataBlock.ToArray());
            Section = sectionHeader;
        }
    }
}
