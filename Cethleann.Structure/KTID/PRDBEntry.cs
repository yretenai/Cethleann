namespace Cethleann.Structure.KTID
{
    public struct PRDBEntry
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public long EntrySize { get; set; }
        public long ContentSize { get; set; }
        public long Size { get; set; }
        public int Type { get; set; }
        public KTIDReference FileKTID { get; set; }
        public KTIDReference TypeInfoKTID { get; set; }
        public RDBFlags Flags { get; set; }
        public KTIDReference UnknownKTID { get; set; }
        public int Unknown { get; set; }
    }
}
