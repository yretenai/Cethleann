using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Texture;
using DragonLib;
using DragonLib.DXGI;

namespace Cethleann.G1
{
    /// <summary>
    ///     G1TextureGroup is a bundle of textures.
    ///     This is how all textures are encoded.
    /// </summary>
    public class G1TextureGroup : IG1Section
    {
        /// <summary>
        ///     Parse G1T from the provided data buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        public G1TextureGroup(Span<byte> data, bool ignoreVersion = false)
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
                var imageData = data.Slice(header.TableOffset + offsets[i]);
                var dataHeader = MemoryMarshal.Read<TextureDataHeader>(imageData);
                var offset = SizeHelper.SizeOf<TextureDataHeader>();
                TextureExtraDataHeader? extra = null;
                if (dataHeader.Flags.HasFlag(TextureFlags.ExtraData))
                {
                    extra = MemoryMarshal.Read<TextureExtraDataHeader>(imageData.Slice(offset));
                    offset += SizeHelper.SizeOf<TextureExtraDataHeader>();
                }

                var (width, height, mips, _) = UnpackWHM(dataHeader);
                var size = dataHeader.Type switch
                {
                    TextureType.R8G8B8A8 => (width * height * 4),
                    TextureType.B8G8R8A8 => (width * height * 4),
                    TextureType.BC1 => (width * height / 2),
                    TextureType.BC5 => (width * height),
                    _ => throw new InvalidOperationException($"Format {dataHeader.Type:X} is unknown!")
                };

                var localSize = size;
                for (var j = 1; j < mips; j++)
                {
                    localSize /= 4;
                    size += localSize;
                }

                var block = imageData.Slice(offset, size);
                Textures.Add((usage[i], dataHeader, extra, new Memory<byte>(block.ToArray())));
            }
        }

        /// <summary>
        ///     List of textures found in this bundle
        /// </summary>
        public List<(TextureUsage usage, TextureDataHeader header, TextureExtraDataHeader? extra, Memory<byte> blob)> Textures { get; } = new List<(TextureUsage, TextureDataHeader, TextureExtraDataHeader?, Memory<byte>)>();

        /// <inheritdoc />
        public int SupportedVersion { get; } = 60;

        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }

        /// <summary>
        ///     Unpacks Width, Height, Mip Count, and DXGI Format from a G1T data header.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static (int width, int height, int mips, DXGIPixelFormat format) UnpackWHM(TextureDataHeader header)
        {
            var width = (int) Math.Pow(2, header.PackedDimensions & 0xF);
            var height = (int) Math.Pow(2, header.PackedDimensions >> 4);
            var mips = header.MipCount >> 4;
            var format = header.Type switch
            {
                TextureType.R8G8B8A8 => DXGIPixelFormat.R8G8B8A8_UNORM,
                TextureType.B8G8R8A8 => DXGIPixelFormat.B8G8R8A8_UNORM,
                TextureType.BC1 => DXGIPixelFormat.BC1_UNORM,
                TextureType.BC5 => DXGIPixelFormat.BC3_UNORM,
                _ => DXGIPixelFormat.R8G8B8A8_UNORM
            };

            return (width, height, mips, format);
        }
    }
}
