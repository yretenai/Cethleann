using System;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Model;
using DragonLib;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    ///     Skeleton Section of G1M models
    /// </summary>
    public class G1MS : IG1Section
    {
        /// <summary>
        ///     Model Skeleton Data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        public G1MS(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != DataType.ModelSkeleton) throw new InvalidOperationException("Not an G1MS stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MS version {Section.Version.ToVersion()} is not supported!");

            var header = MemoryMarshal.Read<ModelSkeletonHeader>(data);
            Helper.Assert(header.SkeletonCount == 1, "SkeletonCount == 1");
            BoneIndices = MemoryMarshal.Cast<byte, short>(data.Slice(SizeHelper.SizeOf<ModelSkeletonHeader>(), header.BoneTableCount * 2)).ToArray();
            BoneIndicesFiltered = BoneIndices.Where(x => x != -1).ToArray();
            Bones = MemoryMarshal.Cast<byte, ModelSkeletonBone>(data.Slice(header.DataOffset - SizeHelper.SizeOf<ResourceSectionHeader>(), header.BoneCount * SizeHelper.SizeOf<ModelSkeletonBone>())).ToArray();
        }

        /// <summary>
        ///     Bone Remap Index
        /// </summary>
        public short[] BoneIndices { get; }

        /// <summary>
        ///     BoneIndices but without -1
        /// </summary>
        public short[] BoneIndicesFiltered { get; }

        /// <summary>
        ///     lsof bones
        /// </summary>
        public ModelSkeletonBone[] Bones { get; }

        /// <inheritdoc />
        public int SupportedVersion { get; } = 32;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }

        /// <summary>
        ///     Returns the remapped bone at the specified index.
        ///     If the index is not remapped, returns null.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ModelSkeletonBone? AtIndex(int i)
        {
            var index = BoneIndices[i];
            if (index == -1) return null;

            return Bones[index];
        }

        /// <summary>
        ///     Returns the remapped bone at the specified index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ModelSkeletonBone AtFilteredIndex(int i) => Bones[BoneIndices[i]];
    }
}
