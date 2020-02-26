namespace Cethleann.Structure.KTID
{
    public struct OBJDBProperty
    {
        public OBJDBPropertyType TypeId { get; set; }
        public int Count { get; set; }
        public KTIDReference PropertyKTID { get; set; }

        public static implicit operator uint(OBJDBProperty reference)
        {
            return reference.PropertyKTID;
        }

        public static implicit operator KTIDReference(OBJDBProperty reference)
        {
            return reference.PropertyKTID;
        }
    }
}
