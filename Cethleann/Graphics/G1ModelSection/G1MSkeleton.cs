using System;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Model;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Graphics.G1ModelSection
{
    /// <summary>
    ///     Skeleton Section of G1M models
    /// </summary>
    [PublicAPI]
    public class G1MSkeleton : IKTGLSection
    {
        /// <summary>
        ///     Model Skeleton Data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        public G1MSkeleton(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != DataType.G1MS) throw new InvalidOperationException("Not an G1MS stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MS version {Section.Version.ToVersion()} is not supported!");

            Header = MemoryMarshal.Read<ModelSkeletonHeader>(data);
            BoneIndices = MemoryMarshal.Cast<byte, short>(data.Slice(SizeHelper.SizeOf<ModelSkeletonHeader>(), Header.BoneTableCount)).ToArray();
            Bones = MemoryMarshal.Cast<byte, ModelSkeletonBone>(data.Slice(Header.DataOffset - SizeHelper.SizeOf<ResourceSectionHeader>(), Header.BoneCount * SizeHelper.SizeOf<ModelSkeletonBone>())).ToArray();
        }

        /// <summary>
        ///     Format Header
        /// </summary>
        public ModelSkeletonHeader Header { get; set; }

        /// <summary>
        ///     Bone Remap Index
        /// </summary>
        public short[] BoneIndices { get; }

        /// <summary>
        ///     lsof bones
        /// </summary>
        public ModelSkeletonBone[] Bones { get; }
        
        /// <inheritdoc />
        public int SupportedVersion { get; } = 32;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
    }
}
