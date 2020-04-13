using DragonLib.Imaging.DXGI;

namespace Cethleann.Structure.Resource.Texture
{
    public struct ScreenLayoutTextureImage
    {
        public ScreenLayoutTextureFormat Format { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        public int Unknown5 { get; set; }
        public int Unknown6 { get; set; }
        public int Size { get; set; }
        public int Unknown7 { get; set; }
        public int Unknown8 { get; set; }
        public int Unknown9 { get; set; }
        public int Unknown10 { get; set; }
        public int Unknown11 { get; set; }
        public int Unknown12 { get; set; }
        public int Unknown13 { get; set; }
        public int Unknown14 { get; set; }
        public int Unknown15 { get; set; }
        public int Unknown16 { get; set; }

        public DXGIPixelFormat ToDXGI() =>
            Format switch
            {
                ScreenLayoutTextureFormat.A8R8G8B8 => DXGIPixelFormat.R8G8B8A8_UNORM,
                ScreenLayoutTextureFormat.R8G8B8A8 => DXGIPixelFormat.R8G8B8A8_UNORM,
                ScreenLayoutTextureFormat.BC3 => DXGIPixelFormat.BC3_UNORM,
                _ => DXGIPixelFormat.UNKNOWN
            };
    }
}
