using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;
using JetBrains.Annotations;

namespace Cethleann.Graphics.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Parses submesh data
    /// </summary>
    [PublicAPI]
    public class G1MGSubMesh : IG1MGSection
    {
        internal G1MGSubMesh(Span<byte> data, ModelSection section)
        {
            Section = section;
            SubMeshes = MemoryMarshal.Cast<byte, ModelGeometrySubMesh>(data).ToArray();
        }

        /// <summary>
        ///     Assembles a series of faces into a polygon.
        /// </summary>
        public ModelGeometrySubMesh[] SubMeshes { get; }

        /// <inheritdoc />
        public ModelGeometrySectionType Type => ModelGeometrySectionType.SubMesh;

        /// <inheritdoc />
        public ModelSection Section { get; }
    }
}
