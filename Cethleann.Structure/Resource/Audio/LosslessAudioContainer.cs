namespace Cethleann.Structure.Resource.Audio
{
    public struct LosslessAudioContainer
    {
        public ResourceSectionHeader Header { get; set; }
        public int DataStartPointer { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int SoundPointer { get; set; }
    }
}
