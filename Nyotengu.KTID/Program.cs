using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Graphics;
using Cethleann.KTID;
using Cethleann.Structure;
using Cethleann.Structure.KTID;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Texture;
using DragonLib;
using DragonLib.CLI;
using DragonLib.IO;

namespace Nyotengu.KTID
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Nyotengu");
            var flags = CommandLineFlags.ParseFlags<KTIDFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;

            var ndb = new NDB();
            if (!string.IsNullOrEmpty(flags.NDBPath) && File.Exists(flags.NDBPath)) ndb = new NDB(File.ReadAllBytes(flags.NDBPath));

            var objdb = new OBJDB(File.ReadAllBytes(flags.OBJDBPath), ndb);
            var filelist = Cethleann.ManagedFS.Nyotengu.LoadKTIDFileList(flags.FileList, flags.GameId);

            var textureSection = new ResourceSectionHeader
            {
                Magic = DataType.TextureGroup,
                Version = 0x30303630
            };

            var textureHeader = new TextureGroupHeader
            {
                System = objdb.Header.System
            };

            foreach (var ktid in flags.Paths)
            {
                if (!File.Exists(ktid)) continue;
                var ktidgroup = new KTIDTextureSet(File.ReadAllBytes(ktid));
                var ktidsystem = ktidgroup.Textures.Select(x => objdb.Entries.TryGetValue(x, out var tuple) ? tuple.properties : default).ToArray();
                var texturePaths = ktidsystem.SelectMany(x => x.Where(y => y.Key.TypeId == OBJDBPropertyType.KTID).SelectMany(y =>
                {
                    return y.Value.Select(z =>
                    {
                        if (z == null || !(z is KTIDReference reference)) return null;
                        var targetPath = Path.Combine(flags.MaterialFolderPath, reference.GetName(ndb, filelist) ?? $"{reference:x8}");
                        if (!targetPath.EndsWith(".g1t")) targetPath += ".g1t";
                        if (!File.Exists(targetPath)) targetPath = Path.Combine(flags.MaterialFolderPath, $"{reference:x8}.g1t");
                        if (!File.Exists(targetPath)) targetPath = Path.Combine(flags.MaterialFolderPath, $"0x{reference:x8}.g1t");
                        return targetPath;
                    }).ToArray();
                })).ToArray();
                var textureBlobs = new List<Memory<byte>>();
                var textureInfo = new List<int>();
                foreach (var texturePath in texturePaths)
                {
                    if (string.IsNullOrEmpty(texturePath) || !File.Exists(texturePath))
                    {
                        Logger.Error("Nyotengu", $"KTID file {ktid} defines a texture that doesn't exist! {texturePath}");
                        continue;
                    }

                    Logger.Info("Nyotengu", $"Loading {texturePath}...");

                    var texture = new G1TextureGroup(File.ReadAllBytes(texturePath));
                    textureBlobs.AddRange(texture.Textures.Select(x => x.blob));
                    textureInfo.AddRange(texture.Textures.Select(x => (int) x.usage));
                }

                textureHeader.Count = textureBlobs.Count;
                var blockSize = SizeHelper.SizeOf<int>() * textureHeader.Count;
                textureHeader.TableOffset = SizeHelper.SizeOf<ResourceSectionHeader>() + SizeHelper.SizeOf<TextureGroupHeader>() + blockSize;
                var combinedTexture = new Span<byte>(new byte[textureHeader.TableOffset + blockSize + textureBlobs.Sum(x => x.Length)]);
                textureSection.Size = combinedTexture.Length;
                MemoryMarshal.Write(combinedTexture, ref textureSection);
                var offset = SizeHelper.SizeOf<ResourceSectionHeader>();
                MemoryMarshal.Write(combinedTexture.Slice(offset), ref textureHeader);
                offset += SizeHelper.SizeOf<TextureGroupHeader>();
                MemoryMarshal.Cast<int, byte>(textureInfo.ToArray()).CopyTo(combinedTexture.Slice(offset));
                offset = textureHeader.TableOffset;
                var baseOffset = blockSize;
                for (var i = 0; i < textureBlobs.Count; ++i)
                {
                    BinaryPrimitives.WriteInt32LittleEndian(combinedTexture.Slice(offset + i * 4), baseOffset);
                    textureBlobs[i].Span.CopyTo(combinedTexture.Slice(offset + baseOffset));
                    baseOffset += textureBlobs[i].Length;
                }

                var destination = Path.ChangeExtension(ktid, ".g1t");
                Logger.Info("Nyotengu", $"Saving {destination}");
                File.WriteAllBytes(destination, combinedTexture.ToArray());
            }
        }
    }
}
