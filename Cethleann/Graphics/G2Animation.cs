using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Animation;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Graphics
{
    /// <summary>
    ///     Parser for G2A Files
    /// </summary>
    [PublicAPI]
    public class G2Animation
    {
        /// <summary>
        ///     Initialize with buffer
        /// </summary>
        /// <param name="buffer"></param>
        public G2Animation(Span<byte> buffer)
        {
            Section = MemoryMarshal.Read<ResourceSectionHeader>(buffer);
            var offset = SizeHelper.SizeOf<ResourceSectionHeader>();
            Header = MemoryMarshal.Read<AnimationV2Header>(buffer.Slice(offset));
            var packedInfo = BitPacked.Unpack<AnimationV2PackedInfo>(Header.PackedInfo);
            offset += SizeHelper.SizeOf<AnimationV2Header>();
            if (Section.Version.ToVersionI() >= 50)
            {
                Unknown1 = MemoryMarshal.Read<int>(buffer.Slice(offset));
                Logger.Assert(Unknown1 == 0, "Unknown1 == 0");
                offset += 4;
            }

            var packedBoneInfos = MemoryMarshal.Cast<byte, uint>(buffer.Slice(offset, packedInfo.BlobSize)).ToArray();
            offset += packedInfo.BlobSize;
            foreach (var packedBoneInfo in packedBoneInfos)
            {
                var boneInfo = BitPacked.Unpack<AnimationV2BoneInfo>(packedBoneInfo);
                var timingOffset = boneInfo.DataOffset;
                if (Section.Version.ToVersionI() < 50) timingOffset /= 4;
                timingOffset = (offset + timingOffset).AlignReverse(4);
                var splines = new List<(AnimationV2Spline spline, short[] timing)>();
                for (var i = 0; i < boneInfo.SplineCount; ++i)
                {
                    var info = MemoryMarshal.Read<AnimationV2Spline>(buffer.Slice(timingOffset));
                    timingOffset += SizeHelper.SizeOf<AnimationV2Spline>();
                    var timing = MemoryMarshal.Cast<byte, short>(buffer.Slice(timingOffset, info.KeyframeCount * 2));
                    timingOffset = (timingOffset + info.KeyframeCount * 2).Align(4);
                    splines.Add((info, timing.ToArray()));
                }

                Splines.Add((boneInfo, splines));
            }

            offset += Header.TimingSectionSize;
            QuantizedData = MemoryMarshal.Cast<byte, ulong>(buffer.Slice(offset, Header.QuantizedDataCount * 4 * 8)).ToArray();
        }

        /// <summary>
        ///     GResource
        /// </summary>
        public ResourceSectionHeader Section { get; set; }

        /// <summary>
        ///     Underlying Header
        /// </summary>
        public AnimationV2Header Header { get; set; }

        /// <summary>
        ///     V50 + Only!
        /// </summary>
        public int Unknown1 { get; set; }

        /// <summary>
        ///     Animation Spline Track Data
        /// </summary>
        public List<(AnimationV2BoneInfo info, List<(AnimationV2Spline Info, short[] Timing)> Data)> Splines { get; set; } = new List<(AnimationV2BoneInfo, List<(AnimationV2Spline, short[])>)>();

        /// <summary>
        ///     Animation Interpolation Data
        /// </summary>
        public ulong[] QuantizedData { get; set; }
    }
}
