using System;
using System.Runtime.InteropServices;
using Cethleann.Koei.Structure.Resource.Model;
using JetBrains.Annotations;

namespace Cethleann.Koei.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Parses submesh data
    /// </summary>
    [PublicAPI]
    public class G1MGSubMesh : IG1MGSection
    {
        internal G1MGSubMesh(Span<byte> data, ModelGeometrySection section)
        {
            Section = section;
            SubMeshes = MemoryMarshal.Cast<byte, ModelGeometrySubMesh>(data).ToArray();
        }

        /// <summary>
        ///     Assembles a series of faces into a polygon.
        /// </summary>
        public ModelGeometrySubMesh[] SubMeshes { get; }

        /// <inheritdoc />
        public ModelGeometryType Type => ModelGeometryType.SubMesh;

        /// <inheritdoc />
        public ModelGeometrySection Section { get; }
    }
}
