using System;
using System.Collections.Generic;
using Cethleann.ManagedFS;
using Cethleann.Structure.KTID;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    [PublicAPI]
    public static class KTIDExtensions
    {
        public static (OBJDBEntry entry, Dictionary<OBJDBProperty, object?[]> properties)? Dereference(this KTIDReference instance, OBJDB db)
        {
            return db.Entries.TryGetValue(instance.KTID, out var ktidInstance) ? ktidInstance : default;
        }

        public static Memory<byte> Dereference(this KTIDReference instance, Nyotengu nyotengu)
        {
            return nyotengu.ReadEntry(instance.KTID);
        }

        public static string? GetName(this KTIDReference instance, NDB ndb, Dictionary<uint, string> nameList)
        {
            if (ndb.HashMap.TryGetValue(instance, out var name)) return name;
            return !nameList.TryGetValue(instance, out name) ? null : name;
        }
    }
}
