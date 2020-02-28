using Cethleann.Structure.Resource;

namespace Cethleann.Structure.KTID
{
    public struct OBJDBIndex
    {
        public ResourceSectionHeader SectionHeader { get; set; }
        public KTIDReference KTID { get; set; }
        public KTIDReference TypeInfoKTID { get; set; }
        public int PropertyCount { get; set; }

        public static implicit operator OBJDBRecord(OBJDBIndex index)
        {
            return new OBJDBRecord
            {
                SectionHeader = index.SectionHeader,
                KTID = index.KTID,
                TypeInfoKTID = index.TypeInfoKTID,
                PropertyCount = index.PropertyCount
            };
        }
    }
}
