namespace Cethleann.Structure.WHD
{
#pragma warning disable 1591
    public struct KWB2EntryHeader
    {
        public byte Unknown1 { get; set; }
        public KWB2Codec Codec { get; set; }
        public byte Unknown3 { get; set; }
        public byte Streams { get; set; }
    }
#pragma warning restore 1591
}
