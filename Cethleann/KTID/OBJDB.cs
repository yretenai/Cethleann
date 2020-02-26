using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.KTID;
using DragonLib;
using DragonLib.IO;
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
            { OBJDBPropertyType.Float32, CreateDelegate<float>() },
            { OBJDBPropertyType.Int32, CreateDelegate<int>() },
            { OBJDBPropertyType.KTID, CreateDelegate<KTIDReference>() }
        };

        /// <summary>
        ///     Initialize with buffer, and with an optional Name Database
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="ndb"></param>
        public OBJDB(Span<byte> buffer, NDB? ndb = default)
        {
            Header = MemoryMarshal.Read<OBJDBHeader>(buffer);
            var offset = Header.SectionHeader.Size;
            for (var i = 0; i < Header.Count; ++i)
            {
                var entry = MemoryMarshal.Read<OBJDBEntry>(buffer.Slice(offset));
                if (entry.PropertyCount == 0)
                {
                    Logger.Fatal("KTID", "Property count is zero!");
                    continue;
                }

                var propertyBuffer = buffer.Slice(offset + SizeHelper.SizeOf<OBJDBEntry>(), SizeHelper.SizeOf<OBJDBProperty>() * entry.PropertyCount);
                var size = entry.SectionHeader.Size - SizeHelper.SizeOf<OBJDBEntry>() - propertyBuffer.Length;
                var properties = MemoryMarshal.Cast<byte, OBJDBProperty>(propertyBuffer).ToArray();
                var kodBuffer = buffer.Slice(offset + SizeHelper.SizeOf<OBJDBEntry>() + propertyBuffer.Length, size);
                var propertyMap = new Dictionary<OBJDBProperty, object?[]>();
                var kodOffset = 0;
                foreach (var property in properties)
                {
                    if (!PropertyTypeMap.TryGetValue(property.TypeId, out var tuple))
                    {
                        Logger.Fatal("KTID", $"Don't know how to handle property type {property.TypeId}! At offset {kodOffset:X}@{offset:X}");
                        break;
                    }

                    var (propertySize, processor) = tuple;
                    if (property.Count == 0)
                        propertyMap[property] = new object?[0];
                    else
                        propertyMap[property] = processor(kodBuffer.Slice(kodOffset), property.Count);

                    kodOffset = (kodOffset + propertySize * property.Count).Align(4);
                }

                Entries[entry.KTID] = (entry, propertyMap);
                offset += entry.SectionHeader.Size;
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
        public Dictionary<KTIDReference, (OBJDBEntry entry, Dictionary<OBJDBProperty, object?[]> properties)> Entries { get; set; } = new Dictionary<KTIDReference, (OBJDBEntry, Dictionary<OBJDBProperty, object?[]>)>();

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
