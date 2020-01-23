namespace Cethleann.Structure.Resource.Audio
{
#pragma warning disable 1591
    public struct OGGSoundHeader
    {
        public SoundResourceEntry Base { get; set; }
        public int StreamPointer { get; set; }
        public int StreamSize { get; set; }
    }
#pragma warning restore 1591
}
