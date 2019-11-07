using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;
using DragonLib;
using DragonLib.IO;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Parses submesh groups
    /// </summary>
    public class G1MGMesh : IG1MGSection
    {
        /// <summary>
        ///     List of meshes with the related data.
        /// </summary>
        public List<(ModelGeometryMeshGroup meshGroup, List<(string name, ModelGeometryMesh mesh, int[] indexList)> meshes)> Meshes = new List<(ModelGeometryMeshGroup, List<(string, ModelGeometryMesh, int[])>)>();

        internal G1MGMesh(Span<byte> data, ModelGeometrySection section)
        {
            Section = section;

            var offset = 0;
            for (int i = 0; i < section.Count; ++i)
            {
                var group = MemoryMarshal.Read<ModelGeometryMeshGroup>(data.Slice(offset));
                offset += SizeHelper.SizeOf<ModelGeometryMeshGroup>();
                var meshes = new List<(string, ModelGeometryMesh, int[])>();
                for (int j = 0; j < group.SubMeshCount + group.UnknownCount; ++j)
                {
                    var name = data.Slice(offset, 0x10).ReadString();
                    offset += 0x10;
                    var mesh = MemoryMarshal.Read<ModelGeometryMesh>(data.Slice(offset));
                    offset += SizeHelper.SizeOf<ModelGeometryMesh>();
                    Logger.Assert(mesh.IndexCount > 0, "mesh.IndexCount > 0");
                    var indices = MemoryMarshal.Cast<byte, int>(data.Slice(offset, mesh.IndexCount * 4)).ToArray();
                    offset += mesh.IndexCount * 4;
                    meshes.Add((name, mesh, indices));
                }

                Meshes.Add((group, meshes));
            }
        }

        /// <inheritdoc />
        public ModelGeometryType Type => ModelGeometryType.Mesh;

        /// <inheritdoc />
        public ModelGeometrySection Section { get; }
    }
}
