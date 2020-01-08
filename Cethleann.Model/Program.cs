using System;
using System.IO;
using System.Linq;
using Cethleann.DataTables;
using Cethleann.G1;
using Cethleann.G1.G1ModelSection;
using Cethleann.G1.G1ModelSection.G1MGSection;
using Cethleann.Structure.Resource.Texture;
using DragonLib.Imaging;
using DragonLib.Imaging.DXGI;
using DragonLib.IO;

namespace Cethleann.Model
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var container = args.ElementAtOrDefault(0);
            var set = args.ElementAtOrDefault(1);

            if (container == null || !File.Exists(container))
            {
                Logger.Error("CETH", "Usage: Cethleann.Model.exe E32.bin");
                Logger.Error("CETH", "Usage: Cethleann.Model.exe model.g1m textureset.g1t");
                return;
            }

            if (set != null && !File.Exists(set))
            {
                Logger.Error("CETH", $"File does not exist: {set}");
                set = null;
            }

            var destination = Path.ChangeExtension(container, "gltf");
            var texDestination = Path.Combine(Path.GetDirectoryName(container), Path.GetFileNameWithoutExtension(container) + "_textures");
            var kstlDestination = Path.Combine(Path.GetDirectoryName(container), Path.GetFileNameWithoutExtension(container));

            using var file = File.OpenRead(container);
            var containerData = new Span<byte>(new byte[file.Length]);
            file.Read(containerData);

            var g1Model = default(G1Model);
            var g1TextureGroup = default(G1TextureGroup);
            var kslt = default(ScreenLayoutTexture);

            if (containerData.GetDataType() == DataType.Model)
            {
                if (set != null)
                {
                    using var fileset = File.OpenRead(set);
                    var setData = new Span<byte>(new byte[fileset.Length]);
                    fileset.Read(setData);
                    if (setData.GetDataType() == DataType.TextureGroup) g1TextureGroup = new G1TextureGroup(setData);
                }

                g1Model = new G1Model(containerData);
            }
            else if (containerData.IsDataTable())
            {
                var dataTable = new DataTable(containerData);
                var g1ModelData = dataTable.Entries.FirstOrDefault(x => x.Span.GetDataType() == DataType.Model);
                var g1TextureGroupData = dataTable.Entries.FirstOrDefault(x => x.Span.GetDataType() == DataType.TextureGroup);
                var ksltData = dataTable.Entries.FirstOrDefault(x => x.Span.GetDataType() == DataType.ScreenLayoutTexture);
                if (!g1ModelData.IsEmpty) g1Model = new G1Model(g1ModelData.Span);
                if (!g1TextureGroupData.IsEmpty) g1TextureGroup = new G1TextureGroup(g1TextureGroupData.Span);
                if (!ksltData.IsEmpty) kslt = new ScreenLayoutTexture(ksltData.Span);
            }
            else if (containerData.GetDataType() == DataType.TextureGroup)
            {
                g1TextureGroup = new G1TextureGroup(containerData);
            }
            else if (containerData.GetDataType() == DataType.ScreenLayoutTexture)
            {
                kslt = new ScreenLayoutTexture(containerData);
            }

            if (g1TextureGroup != null) SaveG1T(texDestination, g1TextureGroup);

            if (kslt != null) SaveKSLT(kstlDestination, kslt);

            if (g1Model != null) SaveG1M(destination, g1Model, Path.GetFileName(texDestination));
        }

        private static void SaveKSLT(string pathBase, ScreenLayoutTexture kslt)
        {
            if (!Directory.Exists(pathBase)) Directory.CreateDirectory(pathBase);
            for (var i = 0; i < kslt.Entries.Count; ++i)
            {
                var (header, blob) = kslt.Entries[i];
                var name = kslt.Names[i];
                var format = header.ToDXGI();
                Logger.Info("KSLT", $@"{pathBase}\{(name ?? $"{i:X4}")}.tif");

                if (format == DXGIPixelFormat.UNKNOWN)
                {
                    for (var dxgiFormat = DXGIPixelFormat.UNKNOWN + 1; dxgiFormat < DXGIPixelFormat.DXGI_END; ++dxgiFormat) File.WriteAllBytes($@"{pathBase}\{(name ?? $"{i:X4}")}_({dxgiFormat:G}).dds", DXGI.BuildDDS(dxgiFormat, 1, header.Width, header.Height, 1, blob.Span).ToArray());

                    continue;
                }

                try
                {
                    var data = DXGI.DecompressDXGIFormat(blob.Span, header.Width, header.Height, format);
                    if (TiffImage.WriteTiff($@"{pathBase}\{(name ?? $"{i:X4}")}.tif", data, header.Width, header.Height))
                    {
                        i += 1;
                        continue;
                    }
                }
                catch
                {
                    // ignored
                }

                File.WriteAllBytes($@"{pathBase}\{(name ?? $"{i:X4}")}.dds", DXGI.BuildDDS(format, 1, header.Width, header.Height, 1, blob.Span).ToArray());
            }
        }

        public static void SaveG1T(string pathBase, G1TextureGroup group)
        {
            var i = 0;
            if (!Directory.Exists(pathBase)) Directory.CreateDirectory(pathBase);

            foreach (var (_, header, _, blob) in group.Textures)
            {
                var (width, height, mips, format, _) = G1TextureGroup.UnpackWHM(header);
                Logger.Info("G1T", $@"{pathBase}\{i:X4}.tif");
                if (format == DXGIPixelFormat.UNKNOWN)
                {
                    for (var dxgiFormat = DXGIPixelFormat.UNKNOWN + 1; dxgiFormat < DXGIPixelFormat.DXGI_END; ++dxgiFormat) File.WriteAllBytes($@"{pathBase}\{i:X4}_({dxgiFormat:G}).dds", DXGI.BuildDDS(dxgiFormat, mips, width, height, 1, blob.Span).ToArray());

                    continue;
                }

                var size = header.Type switch
                {
                    TextureType.R8G8B8A8 => (width * height * 4),
                    TextureType.B8G8R8A8 => (width * height * 4),
                    TextureType.BC1 => (width * height / 2),
                    TextureType.BC5 => (width * height),
                    TextureType.BC6 => (width * height),
                    _ => -1
                };

                if (size == -1)
                {
                    size = blob.Length;
                }
                else
                {
                    var localSize = size;
                    for (var j = 1; j < mips; j++)
                    {
                        localSize /= 4;
                        size += localSize;
                    }
                }

                var frames = blob.Span.Length / size;

                if (frames == 1)
                    try
                    {
                        var data = DXGI.DecompressDXGIFormat(blob.Span, width, height, format);
                        if (TiffImage.WriteTiff($@"{pathBase}\{i:X4}.tif", data, width, height))
                        {
                            i += 1;
                            continue;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                File.WriteAllBytes($@"{pathBase}\{i:X4}.dds", DXGI.BuildDDS(format, frames != 0 ? 0 : mips, width, height, frames, blob.Span).ToArray());
                i += 1;
            }
        }

        public static void SaveG1M(string pathBase, G1Model model, string texBase)
        {
            var geom = model.GetSection<G1MGeometry>();
            var gltf = model.ExportMeshes(Path.ChangeExtension(pathBase, "bin"), $"{Path.GetFileNameWithoutExtension(pathBase)}.bin", 0, 0, texBase);
            Logger.Info("G1M", pathBase);
            File.WriteAllText(pathBase, gltf.Serialize("Cethleann"));
            using var materialInfo = File.OpenWrite(Path.ChangeExtension(pathBase, "material.txt"));
            materialInfo.SetLength(0);
            var materials = geom.GetSection<G1MGMaterial>();
            using var materialWriter = new StreamWriter(materialInfo);
            for (var index = 0; index < materials.Materials.Count; ++index)
            {
                var (material, textureSet) = materials.Materials[index];
                materialWriter.WriteLine($"Material {index} {{ Count = {material.Count}, Unknowns = [{material.Unknown1}, {material.Unknown2}, {material.Unknown3}] }}");
                foreach (var texture in textureSet) materialWriter.WriteLine($"\tTexture {{ Index = {texture.Index:X4}, Type = {texture.Kind:G}, AlternateType = {texture.AlternateKind:G}, UV Layer = {texture.TexCoord}, Unknowns = [{texture.Unknown4}, {texture.Unknown5}] }}");
            }
        }
    }
}
