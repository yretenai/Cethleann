using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.G1.G1ModelSection.G1MGSection;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Model;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    ///     Geometry Section of G1M models
    /// </summary>
    [PublicAPI]
    public class IktglMg : IKTGLSection
    {
        /// <summary>
        ///     Model Geometry
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="sectionHeader"></param>
        public IktglMg(Span<byte> data, bool ignoreVersion, ResourceSectionHeader sectionHeader)
        {
            if (sectionHeader.Magic != DataType.ModelGeometry) throw new InvalidOperationException("Not an G1MG stream");

            Section = sectionHeader;
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1MG version {Section.Version.ToVersion()} is not supported!");

            Header = MemoryMarshal.Read<ModelGeometryHeader>(data);
            Logger.Assert(Header.ModelType.ToFourCC(true) == "NX_", "ModelType == NX_");

            var offset = SizeHelper.SizeOf<ModelGeometryHeader>();
            while (offset < data.Length)
            {
                var subSectionHeader = MemoryMarshal.Read<ModelGeometrySection>(data.Slice(offset));
                var block = data.Slice(offset + SizeHelper.SizeOf<ModelGeometrySection>(), subSectionHeader.Size - SizeHelper.SizeOf<ModelGeometrySection>());
                offset += subSectionHeader.Size;
                var section = subSectionHeader.Magic switch
                {
                    ModelGeometryType.Socket => (IG1MGSection) new G1MGSocket(block, subSectionHeader),
                    ModelGeometryType.Material => new G1MGMaterial(block, subSectionHeader),
                    ModelGeometryType.ShaderParam => new G1MGShaderParam(block, subSectionHeader),
                    ModelGeometryType.VertexBuffer => new G1MGVertexBuffer(block, subSectionHeader),
                    ModelGeometryType.VertexAttribute => new G1MGVertexAttribute(block, subSectionHeader),
                    ModelGeometryType.Bone => new G1MGBone(block, subSectionHeader),
                    ModelGeometryType.IndexBuffer => new G1MGIndexBuffer(block, subSectionHeader),
                    ModelGeometryType.SubMesh => new G1MGSubMesh(block, subSectionHeader),
                    ModelGeometryType.Mesh => new G1MGMesh(block, subSectionHeader),
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
        public T GetSection<T>() where T : class, IG1MGSection
        {
            return SubSections.FirstOrDefault(x => x is T) as T;
        }
    }
}
