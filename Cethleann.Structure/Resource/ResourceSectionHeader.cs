namespace Cethleann.Structure.Resource
{
public struct ResourceSectionHeader
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public int Size { get; set; }
    }
}
