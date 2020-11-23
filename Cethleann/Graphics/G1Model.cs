using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Graphics.G1ModelSection;
using Cethleann.Structure;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Model;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Graphics
{
    /// <summary>
    ///     G1Model is the main model format
    /// </summary>
    [PublicAPI]
    public class G1Model : IKTGLSection
    {
        /// <summary>
        ///     Initialize with no data.
        /// </summary>
        public G1Model()
        {
            Section = new ResourceSectionHeader
            {
                Magic = DataType.Model,
                Size = -1,
                Version = SupportedVersion.ToVersionA()
            };
            SectionRoot = new PackedResource();
        }

        /// <summary>
        ///     Parse G1M from the provided data buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="parse"></param>
        public G1Model(Span<byte> data, bool ignoreVersion = true, bool parse = true)
        {
            if (!data.Matches(DataType.Model)) throw new InvalidOperationException("Not an G1M stream");

            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1M version {Section.Version.ToVersion()} is not supported!");

            var header = MemoryMarshal.Read<ModelHeader>(data.Slice(0xC));
            SectionRoot = new PackedResource(data.Slice(header.HeaderSize, Section.Size - header.HeaderSize), header.SectionCount);
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var block in SectionRoot.Sections)
            {
                var sectionHeader = MemoryMarshal.Read<ResourceSectionHeader>(block.Span);
                var dataBlock = block.Span.Slice(SizeHelper.SizeOf<ResourceSectionHeader>());
                if (!parse)
                {
                    Sections.Add(new G1MStub(dataBlock, sectionHeader));
                    continue;
                }

                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                var section = sectionHeader.Magic switch
                {
                    DataType.G1MS => (IKTGLSection) new G1MSkeleton(dataBlock, ignoreVersion, sectionHeader),
                    DataType.G1MF => new G1MFormat(dataBlock, ignoreVersion, sectionHeader),
                    DataType.G1MG => new G1MGeometry(dataBlock, ignoreVersion, sectionHeader),
                    DataType.G1MM => new G1MMatrix(dataBlock, ignoreVersion, sectionHeader),
                    DataType.EXTR => new G1Extra(dataBlock, ignoreVersion, sectionHeader),
                    _ => new G1MStub(dataBlock, sectionHeader)
                };

                Sections.Add(section);
            }
        }

        /// <summary>
        ///     Raw data for each section
        /// </summary>
        public PackedResource SectionRoot { get; set; }

        /// <summary>
        ///     Sections found in this model.
        ///     Look for <seealso cref="G1MGeometry" /> for Geometry.
        ///     Look for <seealso cref="G1MSkeleton" /> for Skeleton.
        /// </summary>
        public List<IKTGLSection?> Sections { get; } = new List<IKTGLSection?>();

        /// <inheritdoc />
        public int SupportedVersion { get; } = 37;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }

        /// <summary>
        ///     Write to G1M span using just SectionsRoot
        /// </summary>
        /// <returns></returns>
        public Span<byte> WriteFromRoot()
        {
            var sectionHeaderSize = SizeHelper.SizeOf<ResourceSectionHeader>();
            var size = SizeHelper.SizeOf<ModelHeader>() + sectionHeaderSize + SectionRoot.Sections.Sum(x => x.Length);
            var buffer = new Span<byte>(new byte[size]);
            var modelSectionHeader = Section;
            modelSectionHeader.Size = size;
            MemoryMarshal.Write(buffer, ref modelSectionHeader);
            var offset = sectionHeaderSize;
            var modelHeader = new ModelHeader
            {
                HeaderSize = sectionHeaderSize + SizeHelper.SizeOf<ModelHeader>(),
                Reserved = 0,
                SectionCount = SectionRoot.Sections.Count
            };
            MemoryMarshal.Write(buffer.Slice(offset), ref modelHeader);
            offset = modelHeader.HeaderSize;

            foreach (var data in SectionRoot.Sections)
            {
                data.Span.CopyTo(buffer.Slice(offset));
                offset += data.Length;
            }

            return buffer;
        }

        /// <summary>
        ///     Gets a specific section from the G1M model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? GetSection<T>() where T : class, IKTGLSection => GetSections<T>().FirstOrDefault();

        /// <summary>
        ///     Gets a specific section from the G1M model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetSections<T>() where T : class, IKTGLSection => Sections.OfType<T>();
    }
}
