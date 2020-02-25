namespace Cethleann.Structure.KTID
{
    public struct RDBHeader
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public int HeaderSize { get; set; }
        public DataSystem System { get; set; }
        public int Count { get; set; }
        public KTIDReference NameDatabaseKTID { get; set; }
    }
}
