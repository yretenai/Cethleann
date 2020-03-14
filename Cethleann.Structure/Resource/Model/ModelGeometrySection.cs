namespace Cethleann.Structure.Resource.Model
{
    public struct ModelSection
    {
        public ModelGeometrySectionType Magic { get; set; }
        public ModelSectionType Type { get; set; }
        public int Size { get; set; }
        public int Count { get; set; }
    }
}
