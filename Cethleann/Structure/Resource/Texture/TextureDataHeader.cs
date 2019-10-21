namespace Cethleann.Structure.Resource.Texture
{




#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member    public struct TextureDataHeader
    {
        /// <summary>
        /// SHR 4
        /// </summary>
        public byte MipCount { get; set; }
        public TextureType Type { get; set; }
        /// <summary>
        /// 2 POW value AND 0xF = width
        /// 2 POW value SHR 0x4 = height
        /// </summary>
        public ushort PackedDimensions { get; set; }
        public TextureFlags Flags { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
