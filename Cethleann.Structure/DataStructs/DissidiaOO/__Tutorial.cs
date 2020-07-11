// System.ComponentModel.DataAnnotations is required for StringLength.
using System.ComponentModel.DataAnnotations;
// JetBrains.Annotations is required for PublicAPI.
using JetBrains.Annotations;

namespace Cethleann.Structure.DataStructs.DissidiaOO
{
    // PublicAPI is used to suppress unused code warnings.
    [PublicAPI]
    public struct __Tutorial
    {
        // `long` types are a 64-bit integer, read as 8 bytes.
        public long Int64;
        // `ulong` types are a 64-bit unsigned integer, read as 8 bytes.
        public ulong UnsignedInt64;
        // `int` types are a 32-bit integer, read as 4 bytes.
        public int Int32;
        // `uint` types are a 32-bit unsigned integer, read as 4 bytes.
        public uint UnsignedInt32;
        // `short` types are a 16-bit integer, read as 2 bytes.
        public short Int16;
        // `ushort` types are a 16-bit unsigned integer, read as 2 bytes.
        public ushort UnsignedInt16;
        // `byte` types are a 8-bit unsigned integer, read as 1 byte.
        public byte Int8;
        // `sbyte` types are a 8-bit integer, read as 1 byte.
        public sbyte UnsignedInt8;
        // `bool` types are a 1-bit unsigned integer, read as 1 byte.
        public bool Boolean;
        // `float` types are a single 32-bit floating point number, read as 4 bytes.
        public float Float32;
        // `double` types are a single 64-bit floating point number, read as 8 bytes.
        public double Float64;
        // `decimal` types are a single 128-bit floating point number, read as 16 bytes.
        public decimal Float128;
        // `string` types are UTF-8 encoded strings, with the size prefixed. Read as 4 + string length bytes.
        public string DynamicString;
        // `StringLength` + `string` types are UTF-8 encoded strings, with a fixed size. Read as StringLength bytes.
        // In this case, 256 bytes.
        [StringLength(0x100)]
        public string FixedString;
    }
}
