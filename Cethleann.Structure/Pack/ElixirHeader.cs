namespace Cethleann.Structure.Pack
{
    public struct ElixirHeader
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public int Size { get; set; }
        public int HeaderSize { get; set; }
        public int TableSize { get; set; }
        public int Count { get; set; }
        public DataSystem Platform { get; set; }
    }
}
