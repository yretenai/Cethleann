using System;
using System.Collections.Generic;
using System.Linq;
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
        public static OBJDBStructure? Dereference(this KTIDReference instance, OBJDB db)
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
        /// <param name="nameLists"></param>
        /// <returns></returns>
        public static string? GetName(this KTIDReference instance, NDB ndb, params Dictionary<KTIDReference, string>[] nameLists)
        {
            if (instance == 0) return "NULL";
            if (ndb.HashMap.TryGetValue(instance, out var name) && !string.IsNullOrWhiteSpace(name)) return name;
            return nameLists.Any(nameList => nameList.TryGetValue(instance, out name) && !string.IsNullOrWhiteSpace(name)) ? name : null;
        }
    }
}
