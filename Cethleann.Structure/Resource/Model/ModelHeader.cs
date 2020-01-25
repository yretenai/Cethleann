namespace Cethleann.Structure.Resource.Model
{
    public struct ModelHeader
    {
        public int HeaderSize { get; set; }
        public int Reserved { get; set; }
        public int SectionCount { get; set; }
    }
}
