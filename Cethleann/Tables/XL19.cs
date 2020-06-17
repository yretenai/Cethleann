using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using Cethleann.Structure.Table;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Tables
{
    /// <summary>
    ///     Text Localization files are used to collect text data.
    /// </summary>
    [PublicAPI]
    public class XL19
    {
        /// <summary>
        ///     Initialize with a span buffer
        /// </summary>
        /// <param name="buffer"></param>
        public XL19(Span<byte> buffer)
        {
            var header = MemoryMarshal.Read<XLHeader>(buffer);
            if (header.Magic != ((uint) DataType.XL & 0xFFFF) && header.Version != 0x13) throw new InvalidOperationException("Not an XL stream");

            var offset = (int) header.TableOffset;
            var sz = SizeHelper.SizeOf<XLHeader>();
            Types = MemoryMarshal.Cast<byte, XLType>(buffer.Slice(sz, header.Types)).ToArray();
            for (var i = 0; i < header.Sets; i++)
            {
                List<object?> data = new List<object?>();
                var locOffset = 0;
                var slice = buffer.Slice(offset);
                foreach (var type in Types)
                {
                    switch (type)
                    {
                        case XLType.StringPointer:
                        {
                            data.Add(buffer.Slice(header.TableOffset + SpanHelper.ReadLittleInt(slice, ref locOffset)).ReadStringNonNull());
                            break;
                        }
                        case XLType.Int32:
                        {
                            data.Add(SpanHelper.ReadLittleInt(slice, ref locOffset));
                            break;
                        }
                        case XLType.Int16:
                        {
                            data.Add(SpanHelper.ReadLittleShort(slice, ref locOffset));
                            break;
                        }
                        case XLType.Int8:
                        {
                            data.Add(SpanHelper.ReadSByte(slice, ref locOffset));
                            break;
                        }
                        case XLType.UInt32:
                        {
                            data.Add(SpanHelper.ReadLittleUInt(slice, ref locOffset));
                            break;
                        }
                        case XLType.UInt16:
                        {
                            data.Add(SpanHelper.ReadLittleUShort(slice, ref locOffset));
                            break;
                        }
                        case XLType.UInt8:
                        {
                            data.Add(SpanHelper.ReadByte(slice, ref locOffset));
                            break;
                        }
                        case XLType.Single:
                        {
                            data.Add(SpanHelper.ReadLittleSingle(slice, ref locOffset));
                            break;
                        }
                        case XLType.NOP:
                            data.Add(null);
                            break;
                        default:
                            throw new NotImplementedException($"opcode {type:G}");
                    }
                }

                if (locOffset != header.Width) Logger.Warn("XL", "Offset misaligned!");
                Entries.Add(data);
                offset += header.Width;
            }
        }

        public XLType[] Types { get; set; }

        /// <summary>
        ///     List of string entries in this text blob
        /// </summary>
        public List<List<object?>> Entries { get; set; } = new List<List<object?>>();
    }
}
