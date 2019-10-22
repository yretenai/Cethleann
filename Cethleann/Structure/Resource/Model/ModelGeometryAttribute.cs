namespace Cethleann.Structure.Resource.Model
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct ModelGeometryAttribute
    {
        public short Index { get; set; }
        public short Offset { get; set; }
        public short DataType { get; set; }
        public VertexSemantic Semantic { get; set; }
        public byte Layer { get; }
    }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
