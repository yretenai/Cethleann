using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Texture;
using DragonLib;
using DragonLib.Imaging.DXGI;
using JetBrains.Annotations;

namespace Cethleann.Graphics
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

            Header = MemoryMarshal.Read<TextureGroupHeader>(data.Slice(SizeHelper.SizeOf<ResourceSectionHeader>()));
            var blobSize = sizeof(int) * Header.Count;
            var usage = MemoryMarshal.Cast<byte, TextureUsage>(data.Slice(SizeHelper.SizeOf<ResourceSectionHeader>() + SizeHelper.SizeOf<TextureGroupHeader>(), blobSize));
            var offsets = MemoryMarshal.Cast<byte, int>(data.Slice(Header.TableOffset, blobSize));

            for (var i = 0; i < Header.Count; i++)
            {
                var dataHeader = MemoryMarshal.Read<TextureDataHeader>(data.Slice(Header.TableOffset + offsets[i]));
                var nextOffset = data.Length - Header.TableOffset - offsets[i];
                if (i < Header.Count - 1)
                {
                    nextOffset = offsets[i + 1] - offsets[i];
                    if (dataHeader.Type == TextureType.BrokenETC1)
                    {
                        nextOffset *= 2;
                        offsets[i + 1] = nextOffset;
                    }
                }

                var imageData = data.Slice(Header.TableOffset + offsets[i], nextOffset);
                var extra = new TextureDataHeaderExtended
                {
                    Size = 0xC
                };

                if (dataHeader.ExtraDataVersion > 0)
                {
                    var offset = SizeHelper.SizeOf<TextureDataHeader>();
                    var extraData = new Span<byte>(new byte[SizeHelper.SizeOf<TextureDataHeaderExtended>()]);
                    var size = MemoryMarshal.Read<int>(imageData.Slice(offset));
                    imageData.Slice(offset, size).CopyTo(extraData);
                    extra = MemoryMarshal.Read<TextureDataHeaderExtended>(extraData);
                }

                Textures.Add((usage[i], dataHeader, extra, metaOnly ? Memory<byte>.Empty : new Memory<byte>(imageData.ToArray())));
            }
        }

        /// <summary>
        ///     G1TG Header
        /// </summary>
        public TextureGroupHeader Header { get; set; }

        /// <summary>
        ///     List of textures found in this bundle
        /// </summary>
        public List<(TextureUsage usage, TextureDataHeader header, TextureDataHeaderExtended extended, Memory<byte> blob)> Textures { get; } = new List<(TextureUsage, TextureDataHeader, TextureDataHeaderExtended, Memory<byte>)>();

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
        public static (int width, int height, TexturePackedInfo info, DXGIPixelFormat format) UnpackWHM(TextureDataHeader header, Func<byte, TextureType> rewriteTextureType = null)
        {
            var dimensions = BitPacked.Unpack<TexturePackedSize>(header.PackedDimensions);
            var info = BitPacked.Unpack<TexturePackedInfo>(header.PackedInfo);
            var width = (int) Math.Pow(2, dimensions.Width);
            var height = (int) Math.Pow(2, dimensions.Height);
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

            return (width, height, info, format);
        }
    }
}
