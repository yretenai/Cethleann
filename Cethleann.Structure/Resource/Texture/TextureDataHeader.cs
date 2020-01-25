namespace Cethleann.Structure.Resource.Texture
{
public struct TextureDataHeader
    {
        /// <summary>
        ///     SHR 4
        /// </summary>
        public byte PackedInfo { get; set; }

        public TextureType Type { get; set; }

        /// <summary>
        ///     2 POW value AND 0xF = width
        ///     2 POW value SHR 0x4 = height
        /// </summary>
        public byte PackedDimensions { get; set; }

        public byte Unknown1 { get; set; }

        public byte Unknown2 { get; set; }

        public byte Unknown3 { get; set; }

        public TextureFlags Flags { get; set; }
    }
}
