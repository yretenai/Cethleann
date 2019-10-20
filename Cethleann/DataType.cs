using DragonLib;

namespace Cethleann
{
    /// <summary>
    /// LSOF 
    /// </summary>
    public enum DataType : uint
    {
        /// <summary>
        /// <seealso cref="StructTable"/>
        /// </summary>
        [FileExtension("struct")]
        StructTable = 0x1612_1900,

        /// <summary>
        /// S-Archive?
        /// </summary>
        [FileExtension("sarc")]
        SARC = 'S' << 0 | 'A' << 8 | 'R' << 16 | 'C' << 24,

        /// <summary>
        /// S-Package?
        /// </summary>
        [FileExtension("spkg")]
        SPKG = 'S' << 0 | 'P' << 8 | 'K' << 16 | 'G' << 24,

        /// <summary>
        /// Koei Tecmo Sound Resource
        /// </summary>
        [FileExtension("ktsl2stbin")]
        KTSR = 'K' << 0 | 'T' << 8 | 'S' << 16 | 'R' << 24,

        /// <summary>
        /// Scene
        /// </summary>
        [FileExtension("scene")]
        SCEN = 'S' << 0 | 'C' << 8 | 'E' << 16 | 'N' << 24,

        /// <summary>
        /// WebM Video Container
        /// </summary>
        [FileExtension("webm")]
        WEBM = 0xA3DF_451A,

        /// <summary>
        /// teXt Localization, version 19.
        /// </summary>
        [FileExtension("text")]
        TextLocalization19 = 'X' << 0 | 'L' << 8 | 0x13 << 16 | 0 << 24,

        /// <summary>
        /// Koei Texture Group
        /// </summary>
        [FileExtension("g1t")]
        TextureGroup = 'G' << 24 | '1' << 16 | 'T' << 8 | 'G' << 0,

        /// <summary>
        /// Koei Model Base
        /// </summary>
        [FileExtension("g1m")]
        Model = 'G' << 24 | '1' << 16 | 'M' << 8 | '_' << 0,

        /// <summary>
        /// Koei Model Skeleton
        /// </summary>
        [FileExtension("g1m.skeleton")]
        ModelSkeleton = 'G' << 24 | '1' << 16 | 'M' << 8 | 'S' << 0,

        /// <summary>
        /// Koei Model F?
        /// </summary>
        [FileExtension("g1m.f")]
        ModelF = 'G' << 24 | '1' << 16 | 'M' << 8 | 'F' << 0,

        /// <summary>
        /// Koei Model Geometry
        /// </summary>
        [FileExtension("g1m.geometry")]
        ModelGeometry = 'G' << 24 | '1' << 16 | 'M' << 8 | 'G' << 0,

        /// <summary>
        /// Koei Model Matrices
        /// </summary>
        [FileExtension("g1m.matrix")]
        ModelMatrix = 'G' << 24 | '1' << 16 | 'M' << 8 | 'M' << 0,

        /// <summary>
        /// Koei Model ExtraData.
        /// </summary>
        [FileExtension("g1m.extra")]
        ModelExtra = 'E' << 24 | 'X' << 16 | 'T' << 8 | 'R' << 0,

        /// <summary>
        /// Koei Animation
        /// </summary>
        [FileExtension("g2a")]
        Animation = 'G' << 24 | '2' << 16 | 'A' << 8 | '_' << 0
    }
}