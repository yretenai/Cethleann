namespace Cethleann.Structure.Resource.Texture
{
    public struct TextureDataHeaderExtended
    {
        public int Size { get; set; }
        public TextureExtendedFlags Flags { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    // Needs more testing.
    public enum TextureExtendedFlags : ulong
    {
        /// <summary>
        /// Header is repeated?
        /// </summary>
        SharedHeader = 0x1000_0000_0000_0000
    }
}
