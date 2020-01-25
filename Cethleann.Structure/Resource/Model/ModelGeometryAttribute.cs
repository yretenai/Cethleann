namespace Cethleann.Structure.Resource.Model
{
    public struct ModelGeometryAttribute
    {
        public short Index { get; set; }
        public short Offset { get; set; }
        public VertexDataType DataType { get; set; }
        public VertexSemantic Semantic { get; set; }
        public byte Layer { get; set; }
    }
}
