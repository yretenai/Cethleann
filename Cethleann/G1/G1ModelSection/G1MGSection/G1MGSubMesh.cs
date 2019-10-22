using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Parses submesh data
    /// </summary>
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
