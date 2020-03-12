using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using Cethleann.Structure.KTID;
using DragonLib;
using DragonLib.IO;
using DragonLib.Numerics;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    /// <summary>
    ///     KTID System Object Database
    /// </summary>
    [PublicAPI]
    public class OBJDB
    {
        /// <summary>
        ///     Callback for Property loaders
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        public delegate object?[] PropertyCallbackDelegate(Span<byte> buffer, int count);

        /// <summary>
        ///     Property Loaders
        /// </summary>
        public static Dictionary<OBJDBPropertyType, (int size, PropertyCallbackDelegate processor)> PropertyTypeMap = new Dictionary<OBJDBPropertyType, (int, PropertyCallbackDelegate)>
        {
            { OBJDBPropertyType.Bool, CreateDelegate<bool>() },
            { OBJDBPropertyType.Byte, CreateDelegate<byte>() },
            { OBJDBPropertyType.Int16, CreateDelegate<short>() },
            { OBJDBPropertyType.UInt16, CreateDelegate<ushort>() },
            { OBJDBPropertyType.Int32, CreateDelegate<int>() },
            { OBJDBPropertyType.UInt32, CreateDelegate<uint>() },
            { OBJDBPropertyType.Int64, CreateDelegate<long>() },
            { OBJDBPropertyType.UInt64, CreateDelegate<ulong>() },
            { OBJDBPropertyType.Float32, CreateDelegate<float>() },
            { OBJDBPropertyType.Vector4, CreateDelegate<Vector4>() },
            { OBJDBPropertyType.Vector2, CreateDelegate<Vector2>() },
            { OBJDBPropertyType.Vector3, CreateDelegate<Vector3>() }
        };

        /// <summary>
        ///     Initialize with buffer, and with an optional Name Database
        /// </summary>
        /// <param name="buffer"></param>
        public OBJDB(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<OBJDBHeader>(buffer);
            var offset = Header.SectionHeader.Size;
            for (var i = 0; i < Header.Count; ++i)
            {
                OBJDBRecord record;
                int pinPtr;
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (buffer.Slice(offset).GetDataType())
                {
                    case DataType.OBJDBIndex:
                        record = MemoryMarshal.Read<OBJDBIndex>(buffer.Slice(offset));
                        pinPtr = SizeHelper.SizeOf<OBJDBIndex>();
                        break;
                    case DataType.OBJDBRecord:
                        record = MemoryMarshal.Read<OBJDBRecord>(buffer.Slice(offset));
                        pinPtr = SizeHelper.SizeOf<OBJDBRecord>();
                        break;
                    default:
                        Logger.Fatal("KTID", $"Unable to handle record type at offset {offset:X}");
                        continue;
                }

                if (record.PropertyCount == 0) continue;

                Span<byte> propertyBuffer;
                try
                {
                    propertyBuffer = buffer.Slice(offset + pinPtr, SizeHelper.SizeOf<OBJDBProperty>() * record.PropertyCount);
                }
                catch (Exception e)
                {
                    Logger.Fatal("KTID", e);
                    continue;
                }

                var size = record.SectionHeader.Size - pinPtr - propertyBuffer.Length;
                var properties = MemoryMarshal.Cast<byte, OBJDBProperty>(propertyBuffer).ToArray();
                var kodBuffer = buffer.Slice(offset + pinPtr + propertyBuffer.Length, size);
                var propertyMap = new Dictionary<OBJDBProperty, object?[]>();
                var kodOffset = 0;
                foreach (var property in properties)
                {
                    if (!PropertyTypeMap.TryGetValue(property.TypeId, out var tuple))
                    {
                        Logger.Fatal("KTID", $"Unable to handle property type {property.TypeId}! At offset {kodOffset:X}@{offset:X}");
                        break;
                    }

                    var (propertySize, processor) = tuple;
                    if (property.Count == 0)
                        propertyMap[property] = new object?[0];
                    else
                        propertyMap[property] = processor(kodBuffer.Slice(kodOffset), property.Count);

                    kodOffset += propertySize * property.Count;
                }

                Entries[record.KTID] = new OBJDBStructure(record, propertyMap);
                offset += record.SectionHeader.Size;
                offset = offset.Align(4);
            }
        }

        /// <summary>
        ///     KIDSOBJDB header
        /// </summary>
        public OBJDBHeader Header { get; set; }

        /// <summary>
        ///     Entries in the database
        /// </summary>
        public Dictionary<KTIDReference, OBJDBStructure> Entries { get; set; } = new Dictionary<KTIDReference, OBJDBStructure>();

        /// <summary>
        ///     Helper function to create primitive readers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static (int, PropertyCallbackDelegate) CreateDelegate<T>() where T : struct
        {
            return (SizeHelper.SizeOf<T>(), (b, c) => MemoryMarshal.Cast<byte, T>(b.Slice(0, SizeHelper.SizeOf<T>() * c)).ToArray().Select(x => (object?) x).ToArray());
        }
    }
}
