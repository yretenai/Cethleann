using System;
using Cethleann.Archive;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    [PublicAPI]
    public class KTIDTypeInfoAttribute : Attribute
    {
        public KTIDTypeInfoAttribute(uint hash)
        {
            Hash = hash;
            Name = null;
        }

        public KTIDTypeInfoAttribute(string name)
        {
            Name = name;
            Hash = RDB.Hash(name);
        }

        public KTIDTypeInfoAttribute(uint hash, string friendlyName)
        {
            Hash = hash;
            Name = friendlyName;
        }

        public string Name { get; }

        public uint Hash { get; }

        public override object TypeId => Hash;

        public override bool Equals(object obj)
        {
            return obj switch
            {
                uint hash => (hash == Hash),
                string name => (name.Equals(Name) || RDB.Hash(name) == Hash),
                KTIDTypeInfoAttribute ktid => (ktid.Hash == Hash && ktid.Name?.Equals(Name) == true),
                _ => base.Equals(obj)
            };
        }

        public override bool Match(object obj)
        {
            return obj switch
            {
                KTIDTypeInfoAttribute ktid => (ktid.Hash == Hash),
                _ => false
            };
        }

        public override int GetHashCode() => HashCode.Combine(Hash, Name ?? Hash.ToString("x8"));

        public override bool IsDefaultAttribute() => Hash == 0;

        public override string ToString() => $"[{(Name ?? Hash.ToString("x8"))}] {Hash:x8}";
    }
}
