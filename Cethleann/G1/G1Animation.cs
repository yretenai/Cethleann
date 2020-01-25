using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Animation;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.G1
{
    /// <summary>
    ///     Parser for G1A Files
    /// </summary>
    [PublicAPI]
    public class G1Animation
    {
        /// <summary>
        ///     Initialize with buffer
        /// </summary>
        /// <param name="buffer"></param>
        public G1Animation(Span<byte> buffer)
        {
            Section = MemoryMarshal.Read<ResourceSectionHeader>(buffer);
            var offset = SizeHelper.SizeOf<ResourceSectionHeader>();
            Header = MemoryMarshal.Read<AnimationV1Header>(buffer.Slice(offset));
            Logger.Assert(Header.Unknown2 == 1, "Header.Unknown2 == 1");
            Logger.Assert(Header.Reserved3 == 1, "Header.Reserved3 == 1");
            offset += SizeHelper.SizeOf<AnimationV1Header>();
            var table = MemoryMarshal.Cast<byte, int>(buffer.Slice(offset, Header.Count * 4 * 2));
            offset -= 4;
            for (var i = 0; i < Header.Count; ++i)
            {
                var boneId = table[i * 2];
                var pointer = table[i * 2 + 1] * 0x10;
                var type = MemoryMarshal.Read<AnimationV1SplineType>(buffer.Slice(offset + pointer));
                var count = type switch
                {
                    AnimationV1SplineType.WorldS => 3,
                    AnimationV1SplineType.WorldR => 4,
                    AnimationV1SplineType.WorldT => 3,
                    AnimationV1SplineType.WorldRT => 7,
                    AnimationV1SplineType.WorldST => 7,
                    AnimationV1SplineType.WorldRST => 10,
                    AnimationV1SplineType.WorldRS => 7,
                    _ => -1
                };

                var data = new List<float[]>();
                var time = new List<float[]>();
                if (count > 0)
                {
                    var curveTable = MemoryMarshal.Cast<byte, int>(buffer.Slice(offset + pointer + 4, count * 4 * 2));
                    for (var j = 0; j < count; ++j)
                    {
                        var frames = curveTable[j * 2];
                        var curvePointer = offset + pointer + (curveTable[j * 2 + 1] << 4);
                        var curveSize = frames * 4 * 4;
                        data.Add(MemoryMarshal.Cast<byte, float>(buffer.Slice(curvePointer, curveSize)).ToArray());
                        time.Add(MemoryMarshal.Cast<byte, float>(buffer.Slice(curvePointer + curveSize, frames * 4)).ToArray());
                    }
                }

                if (count == -1) Logger.Warn("G1A", $"Unhandled Spline Type: {type}");

                Splines.Add((boneId, type, data, time));
            }
        }

        /// <summary>
        ///     GResource
        /// </summary>
        public ResourceSectionHeader Section { get; set; }

        /// <summary>
        ///     Underlying Header
        /// </summary>
        public AnimationV1Header Header { get; set; }

        /// <summary>
        ///     Animation Spline Track Data
        /// </summary>
        public List<(int Id, AnimationV1SplineType Type, List<float[]> Data, List<float[]> Time)> Splines { get; set; } = new List<(int, AnimationV1SplineType, List<float[]>, List<float[]>)>();
    }
}
