namespace Cethleann.Structure.Resource.Audio
{
#pragma warning disable 1591
    public struct OGGSoundHeader
    {
        public SoundResourceEntry Base { get; set; }
        public int HeaderSize { get; set; }
        public int KTSSSize { get; set; }
    }
#pragma warning restore 1591
}
