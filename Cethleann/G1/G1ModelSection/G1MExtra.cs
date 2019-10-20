using Cethleann.Structure.Art;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// Extra Section of G1M models
    /// </summary>
    public class G1MExtra : IG1Section
    {

        /// <inheritdoc/>
        public int SupportedVersion { get; } = 10;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }
    }
}
