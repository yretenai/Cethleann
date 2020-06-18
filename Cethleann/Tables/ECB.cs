using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Cethleann.Structure.Table;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Tables
{
    /// <summary>
    ///     DFFNT ECB
    /// </summary>
    [PublicAPI]
    public class ECB
    {
        /// <summary>
        ///     Initialize with given buffer
        /// </summary>
        /// <param name="buffer"></param>
        public ECB(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<ECBHeader>(buffer);
            for (var i = 0; i < Header.EntryCount; ++i) Entries.Add(buffer.Slice(SizeHelper.SizeOf<ECBHeader>().Align(0x10) + i * Header.Stride, Header.Stride).ToArray());

            DynamicData = buffer.Slice(Header.DynamicDataPointer, Header.TotalSize - Header.DynamicDataPointer).ToArray();
        }

        /// <summary>
        ///     The header.
        /// </summary>
        public ECBHeader Header { get; set; }

        /// <summary>
        ///     Dynamic Data Buffer, such as strings
        /// </summary>
        public Memory<byte> DynamicData { get; }

        /// <summary>
        ///     Lsof Entries found in the struct table
        /// </summary>
        public List<Memory<byte>> Entries { get; } = new List<Memory<byte>>();

        /// <summary>
        ///     Cast all entries to the specified struct type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> Cast<T>() => Cast(typeof(T)).Cast<T>().ToList();

        /// <summary>
        ///     Cast all entries to the specified struct type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public List<object?> Cast(Type t)
        {
            var entries = new List<object?>();
            var properties = t.GetProperties();
            var count = Math.Min(Header.FieldCount, properties.Length);
            foreach (var buffer in Entries)
            {
                var slice = buffer.Span;
                var instance = Activator.CreateInstance(t);
                var localOffset = 0;
                for (var index = 0; index < count; index++)
                {
                    var property = properties[index];
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
                        case "System.Boolean":
                            value = SpanHelper.ReadByte(slice, ref localOffset) == 1;
                            break;
                        case "System.String":
                        {
                            var offset = SpanHelper.ReadLittleInt(slice, ref localOffset);
                            value = null;
                            var resolvedSize = DynamicData.Length - SizeHelper.SizeOf<ECBStringHeader>();
                            if (offset >= 0 && resolvedSize >= 0 && offset < resolvedSize)
                            {
                                var stringHeader = MemoryMarshal.Read<ECBStringHeader>(DynamicData.Span.Slice(offset));
                                Logger.Assert(stringHeader.CharSet == ECBCharSet.UTF8, "CharSet == ECBCharSet.UTF8");
                                if (stringHeader.Size > 1) value = Encoding.UTF8.GetString(DynamicData.Span.Slice(offset + SizeHelper.SizeOf<ECBStringHeader>(), stringHeader.Size - 1));
                            }

                            break;
                        }
                        default:
                            throw new NotImplementedException(type.FullName);
                    }

                    property.SetValue(instance, value);
                }

                entries.Add(instance);
            }

            return entries;
        }
    }
}
