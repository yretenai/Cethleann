using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Cethleann.Structure.Table;
using DragonLib.IO;
using JetBrains.Annotations;
using DataType = Cethleann.Structure.DataType;

namespace Cethleann.Tables
{
    /// <summary>
    ///     Serialization files are used to collect struct data.
    /// </summary>
    [PublicAPI]
    public class XL20
    {
        /// <summary>
        ///     Initialize with a span buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="t"></param>
        public XL20(Span<byte> buffer, Type t)
        {
            var header = MemoryMarshal.Read<XLHeader>(buffer);
            if (header.Magic != ((uint) DataType.XL & 0xFFFF) && header.Version != 0x14) throw new InvalidOperationException("Not an XL stream");
            UnderlyingType = t;

            var offset = (int) header.TableOffset;
            var properties = t.GetProperties();
            for (var i = 0; i < header.FileSize; i++)
            {
                var instance = Activator.CreateInstance(t);
                var localOffset = 0;
                var slice = buffer.Slice(offset);
                foreach (var property in properties)
                {
                    var type = property.PropertyType;
                    object? value;
                    switch (type.FullName)
                    {
                        case "System.Int64":
                            value = SpanHelper.ReadLittleLong(slice, ref localOffset);
                            break;
                        case "System.Int32":
                            value = SpanHelper.ReadLittleInt(slice, ref localOffset);
                            break;
                        case "System.Int16":
                            value = SpanHelper.ReadLittleShort(slice, ref localOffset);
                            break;
                        case "System.UInt64":
                            value = SpanHelper.ReadLittleULong(slice, ref localOffset);
                            break;
                        case "System.UInt32":
                            value = SpanHelper.ReadLittleUInt(slice, ref localOffset);
                            break;
                        case "System.UInt16":
                            value = SpanHelper.ReadLittleUShort(slice, ref localOffset);
                            break;
                        case "System.Int8":
                        case "System.SByte":
                            value = SpanHelper.ReadSByte(slice, ref localOffset);
                            break;
                        case "System.UInt8":
                        case "System.Byte":
                            value = SpanHelper.ReadByte(slice, ref localOffset);
                            break;
                        case "System.Single":
                            value = SpanHelper.ReadLittleSingle(slice, ref localOffset);
                            break;
                        case "System.Double":
                            value = SpanHelper.ReadLittleDouble(slice, ref localOffset);
                            break;
                        case "System.Decimal":
                            value = SpanHelper.ReadLittleDecimal(slice, ref localOffset);
                            break;
                        case "System.Boolean":
                            value = SpanHelper.ReadByte(slice, ref localOffset) == 1;
                            break;
                        case "System.String":
                        {
                            var stringSize = property.GetCustomAttribute<StringLengthAttribute>();
                            var size = stringSize?.MaximumLength ?? SpanHelper.ReadLittleInt(slice, ref localOffset);
                            value = size > 0 ? Encoding.UTF8.GetString(slice.Slice(localOffset, size)) : null;
                            if (stringSize != null && value is string stringValue)
                            {
                                value = stringValue.TrimEnd('\u0000');
                            }
                            localOffset += size;
                            break;
                        }
                        default:
                            throw new NotImplementedException(type.FullName);
                    }

                    property.SetValue(instance, value);
                }

                Entries.Add(instance);
                offset += localOffset;
            }
        }

        public Type UnderlyingType { get; set; }

        /// <summary>
        ///     List of entries in this blob
        /// </summary>
        public List<object?> Entries { get; set; } = new List<object?>();
    }
}
