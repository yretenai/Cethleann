namespace Cethleann.Structure.Resource.Model
{
public struct ModelGeometrySubMesh
    {
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int BoneTableIndex { get; set; }
        public int BoneIndex { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        public int MaterialIndex { get; set; }
        public int BufferIndex { get; set; }
        public int Unknown5 { get; set; }
        public SubMeshFormat Format { get; set; }
        public int VertexOffset { get; set; }
        public int VertexCount { get; set; }
        public int FaceOffset { get; set; }
        public int FaceCount { get; set; }
    }

}
