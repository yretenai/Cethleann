using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Texture;
using DragonLib;
using DragonLib.Imaging.DXGI;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.G1
{
    /// <summary>
    ///     G1TextureGroup is a bundle of textures.
    ///     This is how all textures are encoded.
    /// </summary>
    [PublicAPI]
    public class G1TextureGroup : IKTGLSection
    {
        /// <summary>
        ///     Parse G1T from the provided data buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="metaOnly">only parse metadata</param>
        public G1TextureGroup(Span<byte> data, bool ignoreVersion = true, bool metaOnly = false)
        {
            if (!data.Matches(DataType.TextureGroup)) throw new InvalidOperationException("Not an G1T stream");

            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1T version {Section.Version.ToVersion()} is not supported!");

            var header = MemoryMarshal.Read<TextureGroupHeader>(data.Slice(SizeHelper.SizeOf<ResourceSectionHeader>()));
            var blobSize = sizeof(int) * header.EntrySize;
            var usage = MemoryMarshal.Cast<byte, TextureUsage>(data.Slice(SizeHelper.SizeOf<ResourceSectionHeader>() + SizeHelper.SizeOf<TextureGroupHeader>(), blobSize));
            var offsets = MemoryMarshal.Cast<byte, int>(data.Slice(header.TableOffset, blobSize));

            for (var i = 0; i < header.EntrySize; i++)
            {
                var nextOffset = data.Length - header.TableOffset - offsets[i];
                if (i < header.EntrySize - 1) nextOffset = offsets[i + 1] - offsets[i];
                var imageData = data.Slice(header.TableOffset + offsets[i], nextOffset);
                var dataHeader = MemoryMarshal.Read<TextureDataHeader>(imageData);
                var offset = SizeHelper.SizeOf<TextureDataHeader>();
                var extra = new TextureExtraDataHeader
                {
                    Size = 0xC
                };

                if (dataHeader.Flags.HasFlag(TextureFlags.ExtraData))
                {
                    extra = MemoryMarshal.Read<TextureExtraDataHeader>(imageData.Slice(offset));
                    offset += extra.Size;
                }

                if (dataHeader.Type.ToString("G") == dataHeader.Type.ToString("D"))
                {
                    Logger.Warn("G1T", $"Texture Type {dataHeader.Type:X} at offset {(header.TableOffset + offsets[i]):X16} (entry {i}) is unsupported!");
                }

                var imagePixelData = Memory<byte>.Empty;
                if (!metaOnly) imagePixelData = new Memory<byte>(imageData.Slice(offset).ToArray());

                Textures.Add((usage[i], dataHeader, extra, imagePixelData));
            }
        }

        /// <summary>
        ///     List of textures found in this bundle
        /// </summary>
        public List<(TextureUsage usage, TextureDataHeader header, TextureExtraDataHeader extra, Memory<byte> blob)> Textures { get; } = new List<(TextureUsage, TextureDataHeader, TextureExtraDataHeader, Memory<byte>)>();

        /// <inheritdoc />
        public int SupportedVersion { get; } = 60;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }

        /// <summary>
        ///     Unpacks Width, Height, Mip Count, and DXGI Format from a G1T data header.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="rewriteTextureType"></param>
        /// <returns></returns>
        public static (int width, int height, int mips, DXGIPixelFormat format, int system) UnpackWHM(TextureDataHeader header, Func<byte, TextureType> rewriteTextureType = null)
        {
            var width = (int) Math.Pow(2, header.PackedDimensions & 0xF);
            var height = (int) Math.Pow(2, header.PackedDimensions >> 4);
            var mips = header.MipCount >> 4;
            var system = header.MipCount & 0xF;
            var type = rewriteTextureType?.Invoke((byte) header.Type) ?? header.Type;
            var format = type switch
            {
                TextureType.R8G8B8A8 => DXGIPixelFormat.R8G8B8A8_UNORM,
                TextureType.B8G8R8A8 => DXGIPixelFormat.B8G8R8A8_UNORM,
                TextureType.BC1 => DXGIPixelFormat.BC1_UNORM,
                TextureType.BC5 => DXGIPixelFormat.BC3_UNORM,
                TextureType.BC6 => DXGIPixelFormat.BC6H_UF16,
                _ => DXGIPixelFormat.UNKNOWN
            };

            return (width, height, mips, format, system);
        }
    }
}
