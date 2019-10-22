using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    /// </summary>
    public class G1MGSubMesh : IG1MGSection
    {
        internal G1MGSubMesh(Span<byte> data, ModelGeometrySection section)
        {
            Section = section;
            SubMeshes = MemoryMarshal.Cast<byte, ModelGeometrySubMesh>(data).ToArray();
        }

        public ModelGeometrySubMesh[] SubMeshes { get; }

        /// <inheritdoc />
        public ModelGeometryType Type => ModelGeometryType.SubMesh;

        /// <inheritdoc />
        public ModelGeometrySection Section { get; }
    }
}
