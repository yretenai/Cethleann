namespace Cethleann.Structure.Resource.Audio
{
    public struct NamedSoundHeader
    {
        public SoundResourceEntry Base { get; set; }
        public int Unknown1 { get; set; }
        public int Count { get; set; }
        public int PointersTablePointer { get; set; }
    }
}
