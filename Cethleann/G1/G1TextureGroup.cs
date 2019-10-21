using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Texture;
using DragonLib;
using DragonLib.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cethleann.G1
{
    /// <summary>
    /// G1TextureGroup is a bundle of textures.
    /// This is how all textures are encoded.
    /// </summary>
    public class G1TextureGroup : IG1Section
    {
        /// <summary>
        /// List of textures found in this bundle
        /// </summary>
        public List<(TextureUsage usage, TextureDataHeader header, TextureExtraDataHeader? extra, Memory<byte> blob)> Textures { get; } = new List<(TextureUsage, TextureDataHeader, TextureExtraDataHeader?, Memory<byte>)>();

        /// <inheritdoc/>
        public int SupportedVersion { get; } = 60;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }

        /// <summary>
        /// Parse G1T from the provided data buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        public G1TextureGroup(Span<byte> data, bool ignoreVersion = false)
        {
            if (!data.Matches(DataType.TextureGroup))
            {
                throw new InvalidOperationException("Not an G1T stream");
            }

            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion)
            {
                throw new NotSupportedException($"G1T version {Section.Version.ToVersion()} is not supported!");
            }

            TextureGroupHeader header = MemoryMarshal.Read<TextureGroupHeader>(data.Slice(SizeHelper.SizeOf<ResourceSectionHeader>()));
            int blobSize = sizeof(int) * header.EntrySize;
            Span<TextureUsage> usage = MemoryMarshal.Cast<byte, TextureUsage>(data.Slice(SizeHelper.SizeOf<ResourceSectionHeader>() + SizeHelper.SizeOf<TextureGroupHeader>(), blobSize));
            Span<int> offsets = MemoryMarshal.Cast<byte, int>(data.Slice(header.TableOffset, blobSize));

            for (int i = 0; i < header.EntrySize; i++)
            {
                Span<byte> imageData = data.Slice(header.TableOffset + offsets[i]);
                TextureDataHeader dataHeader = MemoryMarshal.Read<TextureDataHeader>(imageData);
                int offset = SizeHelper.SizeOf<TextureDataHeader>();
                TextureExtraDataHeader? extra = null;
                if (dataHeader.Flags.HasFlag(TextureFlags.ExtraData))
                {
                    extra = MemoryMarshal.Read<TextureExtraDataHeader>(imageData.Slice(offset));
                    offset += SizeHelper.SizeOf<TextureExtraDataHeader>();
                }
                (int width, int height, int mips, DXGIPixelFormat _) = UnpackWHM(dataHeader);
                int size;
                switch (dataHeader.Type)
                {
                    case TextureType.R8G8B8A8:
                    case TextureType.B8G8R8A8:
                        size = width * height * 4;
                        break;
                    case TextureType.BC1:
                        size = width * height / 2;
                        break;
                    case TextureType.BC5:
                        size = width * height;
                        break;
                    default:
                        throw new InvalidOperationException($"Format {dataHeader.Type:X} is unknown!");
                }
                int localSize = size;
                for (int j = 1; j < mips; j++)
                {
                    localSize /= 4;
                    size += localSize;
                }
                Span<byte> block = imageData.Slice(offset, size);
                Textures.Add((usage[i], dataHeader, extra, new Memory<byte>(block.ToArray())));
            }
        }

        /// <summary>
        /// Unpacks Width, Height, Mip Count, and DXGI Format from a G1T data header.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static (int width, int height, int mips, DXGIPixelFormat format) UnpackWHM(TextureDataHeader header)
        {
            int width = (int)Math.Pow(2, header.PackedDimensions & 0xF);
            int height = (int)Math.Pow(2, header.PackedDimensions >> 4);
            int mips = header.MipCount >> 4;
            DXGIPixelFormat format = DXGIPixelFormat.R8G8B8A8_UNORM;
            switch (header.Type)
            {
                case TextureType.R8G8B8A8:
                    format = DXGIPixelFormat.R8G8B8A8_UNORM;
                    break;
                case TextureType.B8G8R8A8:
                    format = DXGIPixelFormat.B8G8R8A8_UNORM;
                    break;
                case TextureType.BC1:
                    format = DXGIPixelFormat.BC1_UNORM;
                    break;
                case TextureType.BC5:
                    format = DXGIPixelFormat.BC3_UNORM;
                    break;
            }
            return (width, height, mips, format);
        }
    }
}
