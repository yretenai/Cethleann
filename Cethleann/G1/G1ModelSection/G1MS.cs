using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Model;
using DragonLib;
using DragonLib.IO;
using OpenTK;
using Vector3 = DragonLib.Numerics.Vector3;

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
            if (sectionHeader.Magic != ResourceSection.ModelSkeleton) throw new InvalidOperationException("Not an G1MS stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MS version {Section.Version.ToVersion()} is not supported!");

            var header = MemoryMarshal.Read<ModelSkeletonHeader>(data);
            Logger.Assert(header.SkeletonCount == 1, "SkeletonCount == 1");
            BoneIndices = MemoryMarshal.Cast<byte, short>(data.Slice(SizeHelper.SizeOf<ModelSkeletonHeader>(), header.BoneTableCount)).ToArray();
            Bones = MemoryMarshal.Cast<byte, ModelSkeletonBone>(data.Slice(header.DataOffset - SizeHelper.SizeOf<ResourceSectionHeader>(), header.BoneCount * SizeHelper.SizeOf<ModelSkeletonBone>())).ToArray();

            WorldBones = new ModelSkeletonBone[Bones.Length];
            for (var index = 0; index < Bones.Length; index++)
            {
                var bone = Bones[index];
                if (!bone.HasParent())
                {
                    WorldBones[index] = bone;
                    continue;
                }

                if (bone.Parent > index) throw new InvalidOperationException("Bone calculated before parent bone");

                var parentBone = WorldBones[bone.Parent];
                WorldBones[index] = new ModelSkeletonBone
                {
                    Length = bone.Length,
                    Parent = bone.Parent,
                    Scale = (parentBone.Scale.ToOpenTK() * bone.Scale.ToOpenTK()).ToDragon(),
                    Rotation = (parentBone.Rotation.ToOpenTK() * bone.Rotation.ToOpenTK()).ToDragon()
                };
                var local = parentBone.Rotation.ToOpenTK() * new Quaternion(bone.Position.ToOpenTK(), 0f) * new Quaternion(-parentBone.Rotation.X, -parentBone.Rotation.Y, -parentBone.Rotation.Z, parentBone.Rotation.W);
                WorldBones[index].Position = new Vector3(local.X + parentBone.Position.X, local.Y + parentBone.Position.Y, local.Z + parentBone.Position.Z);
            }
        }

        /// <summary>
        ///     Bone Remap Index
        /// </summary>
        public short[] BoneIndices { get; }

        /// <summary>
        ///     lsof bones
        /// </summary>
        public ModelSkeletonBone[] Bones { get; }

        /// <summary>
        ///     lsof world bones
        /// </summary>
        public ModelSkeletonBone[] WorldBones { get; }

        /// <inheritdoc />
        public int SupportedVersion { get; } = 32;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }
    }
}
