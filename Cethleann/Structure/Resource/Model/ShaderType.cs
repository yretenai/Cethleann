namespace Cethleann
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum ShaderType : short
    {
        Float32 = 0x1,
        Matrix4x4x2 = 0x20 // assumption, it contains 32 floats.
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
