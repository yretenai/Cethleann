using Cethleann.Structure.Resource.Model;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    public interface IG1MGSection
    {
        public ModelGeometryType Type { get; }
        public ModelGeometrySection Section { get; }
    }
}
