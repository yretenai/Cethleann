namespace Cethleann.Structure.WAV
{
#pragma warning disable 1591
    public struct WAVEHeader
    {
        public DataType Magic { get; set; }
        public int Size { get; set; }
        public int Format { get; set; }
    }
#pragma warning restore 1591
}
