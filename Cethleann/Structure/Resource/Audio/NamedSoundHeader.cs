namespace Cethleann.Structure.Resource.Audio
{
#pragma warning disable 1591
    public struct NamedSoundHeader
    {
        public SoundResourceEntry Base { get; set; }
        public int Unknown1 { get; set; }
        public int Count { get; set; }
        public int PointersTablePointer { get; set; }
    }
#pragma warning restore 1591
}
