using System;
using System.Collections.Generic;
using Cethleann.ManagedFS;
using Cethleann.Structure.KTID;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    /// <summary>
    ///     KTID Extensions
    /// </summary>
    [PublicAPI]
    public static class KTIDExtensions
    {
        /// <summary>
        ///     Load a referenced KTID structure.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static (OBJDBEntry entry, Dictionary<OBJDBProperty, object?[]> properties)? Dereference(this KTIDReference instance, OBJDB db)
        {
            return db.Entries.TryGetValue(instance.KTID, out var ktidInstance) ? ktidInstance : default;
        }

        /// <summary>
        ///     Load a referenced KTID file
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="nyotengu"></param>
        /// <returns></returns>
        public static Memory<byte> Dereference(this KTIDReference instance, Nyotengu nyotengu)
        {
            return nyotengu.ReadEntry(instance.KTID);
        }

        /// <summary>
        ///     Attempt to retrieve a KTID file
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ndb"></param>
        /// <param name="nameList"></param>
        /// <returns></returns>
        public static string? GetName(this KTIDReference instance, NDB ndb, Dictionary<uint, string> nameList)
        {
            if (ndb.HashMap.TryGetValue(instance, out var name)) return name;
            return !nameList.TryGetValue(instance, out name) ? null : name;
        }
    }
}
