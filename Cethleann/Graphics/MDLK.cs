using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using Cethleann.Structure.Resource;
using JetBrains.Annotations;

namespace Cethleann.Graphics
{
    /// <summary>
    ///     Decomposes a MDLK buffer to individual files
    /// </summary>
    [PublicAPI]
    public class MDLK
    {
        /// <summary>
        ///     Decomposes a MDLK buffer to individual files
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="InvalidDataException"></exception>
        public MDLK(Span<byte> buffer)
        {
            var header = MemoryMarshal.Read<ResourceSectionHeader>(buffer);
            if (header.Magic != DataType.MDLK) throw new InvalidDataException("Not a KLDM stream");

            var offset = 0x10;
            for (int i = 0; i < header.Size; ++i)
            {
                var magic = buffer.Slice(offset).GetDataType();

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (magic)
                {
                    case DataType.Model:
                    case DataType.Animation:
                    case DataType.Effect:
                    case DataType.EffectManager:
                    {
                        var localHeader = MemoryMarshal.Read<ResourceSectionHeader>(buffer.Slice(offset));
                        Entries.Add(new Memory<byte>(buffer.Slice(offset, localHeader.Size).ToArray()));
                        offset += localHeader.Size;
                    }
                        break;
                    default:
                        throw new NotImplementedException($"MDLK Sub {magic.ToFourCC(false)} not implemented!");
                }
            }
        }

        /// <summary>
        ///     Usually models.
        /// </summary>
        public List<Memory<byte>> Entries { get; } = new List<Memory<byte>>();
    }
}
