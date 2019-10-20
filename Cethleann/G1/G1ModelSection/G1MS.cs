using Cethleann.Structure.Art;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// Skeleton Section of G1M models
    /// </summary>
    public class G1MS : IG1Section
    {

        /// <inheritdoc/>
        public int SupportedVersion { get; } = 29;
        
        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }
    }
}
