using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Graphics.G1ModelSection.G1MGSection;
using Cethleann.Structure;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Model;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Graphics.G1ModelSection
{
    /// <summary>
    ///     Geometry Section of G1M models
    /// </summary>
    [PublicAPI]
    public class G1MGeometry : IKTGLSection
    {
        /// <summary>
        ///     Model Geometry
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        public G1MGeometry(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != DataType.G1MG) throw new InvalidOperationException("Not an G1MG stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MG version {Section.Version.ToVersion()} is not supported!");

            Header = MemoryMarshal.Read<ModelGeometryHeader>(data);

            var offset = SizeHelper.SizeOf<ModelGeometryHeader>();
            while (offset < data.Length)
            {
                var subSectionHeader = MemoryMarshal.Read<ModelSection>(data.Slice(offset));
                var block = data.Slice(offset + SizeHelper.SizeOf<ModelSection>(), subSectionHeader.Size - SizeHelper.SizeOf<ModelSection>());
                offset += subSectionHeader.Size;
                var section = subSectionHeader.Magic switch
                {
                    ModelGeometrySectionType.Socket => (IG1MGSection) new G1MGSocket(block, subSectionHeader),
                    ModelGeometrySectionType.Material => new G1MGMaterial(block, subSectionHeader),
                    ModelGeometrySectionType.ShaderParam => new G1MGShaderParam(block, subSectionHeader),
                    ModelGeometrySectionType.VertexBuffer => new G1MGVertexBuffer(block, subSectionHeader),
                    ModelGeometrySectionType.VertexAttribute => new G1MGVertexAttribute(block, subSectionHeader),
                    ModelGeometrySectionType.Bone => new G1MGBone(block, subSectionHeader),
                    ModelGeometrySectionType.IndexBuffer => new G1MGIndexBuffer(block, subSectionHeader),
                    ModelGeometrySectionType.SubMesh => new G1MGSubMesh(block, subSectionHeader),
                    ModelGeometrySectionType.Mesh => new G1MGMesh(block, subSectionHeader),
                    _ => throw new NotSupportedException($"Can't handle G1MG section {subSectionHeader.Magic:F}")
                };
                SubSections.Add(section);
            }
        }

        /// <summary>
        ///     Header info (bounding box)
        /// </summary>
        public ModelGeometryHeader Header { get; }

        /// <summary>
        ///     List of geometry sections
        /// </summary>
        public List<IG1MGSection> SubSections { get; set; } = new List<IG1MGSection>();

        /// <inheritdoc />
        public int SupportedVersion { get; } = 44;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }

        /// <summary>
        ///     Gets a geometry component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSection<T>() where T : class, IG1MGSection => GetSections<T>().FirstOrDefault();

        /// <summary>
        ///     Gets all geometry components matching
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetSections<T>() where T : class, IG1MGSection => SubSections.OfType<T>();
    }
}
