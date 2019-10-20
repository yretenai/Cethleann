using Cethleann.Structure.Art;
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
            if (!data.Matches(DataType.TextureGroup)) throw new InvalidOperationException("Not an G1T stream");
            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1T version {Section.Version.ToVersion()} is not supported!");
            var header = MemoryMarshal.Read<TextureGroupHeader>(data.Slice(0xC));
            var blobSize = 4 * header.EntrySize;
            var usage = MemoryMarshal.Cast<byte, TextureUsage>(data.Slice(0x28, blobSize));
            var offsets = MemoryMarshal.Cast<byte, int>(data.Slice(header.TableOffset, blobSize));
            
            for(var i = 0; i < header.EntrySize; i++)
            {
                var imageData = data.Slice(header.TableOffset + offsets[i]);
                var dataHeader = MemoryMarshal.Read<TextureDataHeader>(imageData);
                var offset = 8;
                TextureExtraDataHeader? extra = null;
                if(dataHeader.Flags.HasFlag(TextureFlags.ExtraData))
                {
                    extra = MemoryMarshal.Read<TextureExtraDataHeader>(imageData.Slice(offset));
                    offset += 0xC;
                }
                var (width, height, mips, _) = UnpackWHM(dataHeader);
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
                var localSize = size;
                for(var j = 1; j < mips; j++)
                {
                    localSize /= 4;
                    size += localSize;
                }
                var block = imageData.Slice(offset, size);
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
            var width = (int)Math.Pow(2, header.PackedDimensions & 0xF);
            var height = (int)Math.Pow(2, header.PackedDimensions >> 4);
            var mips = header.MipCount >> 4;
            var format = DXGIPixelFormat.R8G8B8A8_UNORM;
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
