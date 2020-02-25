using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    [PublicAPI]
    public class TypeInfo
    {
        public static Lazy<TypeInfo> Instance = new Lazy<TypeInfo>();

        public TypeInfo()
        {
            LoadFromAssembly(Assembly.GetExecutingAssembly());
        }

        public Dictionary<uint, Type> TypeMap { get; } = new Dictionary<uint, Type>();
        public Dictionary<uint, string> NameMap { get; } = new Dictionary<uint, string>();
        public HashSet<uint> NagList { get; } = new HashSet<uint>();

        public void LoadFromAssembly(Assembly asm)
        {
            foreach (var (attr, type) in asm.GetTypes().Where(x => typeof(IKTIDSystemType).IsAssignableFrom(x)).Select(x => (x.GetCustomAttribute<KTIDTypeInfoAttribute>(), x)).Where(x => x.Item1 != null))
            {
                if (TypeMap.ContainsKey(attr.Hash)) Logger.Warn("KTID", $"TypeInfo {attr.Name ?? "undefined"}, {attr.Hash:x8} has already been defined");
                TypeMap[attr.Hash] = type;
                NameMap[attr.Hash] = attr.Name;
            }
        }
    }
}
