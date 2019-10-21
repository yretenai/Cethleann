using Cethleann.Structure.Resource.Model;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    public interface IG1MGSection
    {
        ModelGeometryType Type { get; }
        ModelGeometrySection Section { get; }
    }
}
