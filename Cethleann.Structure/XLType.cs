namespace Cethleann.Structure
{
    public enum XLType : byte
    {
        StringPointer = 0x00,
        Int32 = 0x01,
        Int16 = 0x02,
        Int8 = 0x03,
        UInt32 = 0x04,
        UInt16 = 0x05,
        UInt8 = 0x06,
        Single = 0x07,
        NOP = 0xFF
    }
}
