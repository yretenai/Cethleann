namespace Cethleann.Structure.Resource.Audio
{
    public struct OGGSoundHeader
    {
        public SoundResourceEntry Base { get; set; }
        public int StreamPointer { get; set; }
        public int StreamSize { get; set; }
    }
}
