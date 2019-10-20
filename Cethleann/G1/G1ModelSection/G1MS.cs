using System;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.Art;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// Skeleton Section of G1M models
    /// </summary>
    public class G1MS : IG1Section
    {
        /// <inheritdoc/>
        public int SupportedVersion { get; } = 32;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }

        /// <summary>
        /// Bone Remap Index
        /// </summary>
        public short[] BoneIndices { get; }

        /// <summary>
        /// BoneIndices but without -1
        /// </summary>
        public short[] BoneIndicesFiltered { get; }

        /// <summary>
        /// lsof bones
        /// </summary>
        public ModelSkeletonBone[] Bones { get; }

        /// <summary>
        /// Model Skeleton Data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        public G1MS(Span<byte> data, bool ignoreVersion = false)
        {
            if (!data.Matches(DataType.ModelSkeleton)) throw new InvalidOperationException("Not an G1MS stream");
            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MS version {Section.Version.ToVersion()} is not supported!");

            var header = MemoryMarshal.Read<ModelSkeletonHeader>(data.Slice(0xC));
            Helper.Assert(header.SkeletonCount == 1, "SkeletonCount == 1");
            BoneIndices = MemoryMarshal.Cast<byte, short>(data.Slice(0x1C, header.BoneTableCount * 2)).ToArray();
            BoneIndicesFiltered = BoneIndices.Where(x => x != -1).ToArray();
            Bones = MemoryMarshal.Cast<byte, ModelSkeletonBone>(data.Slice(header.DataOffset, header.BoneCount * 0x30)).ToArray();
        }

        /// <summary>
        /// Returns the remapped bone at the specified index.
        /// If the index is not remapped, returns null.
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
        /// Returns the remapped bone at the specified index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ModelSkeletonBone AtFilteredIndex(int i)
        {
            return Bones[BoneIndices[i]];
        }
    }
}
