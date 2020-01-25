using DragonLib;

namespace Cethleann.Structure
{
    /// <summary>
    ///     Magic Values for most files
    /// </summary>
    public enum DataType : uint
    {
        /// <summary>
        ///     Used as a dummy
        /// </summary>
        [FileExtension("bin")]
        None = 0,

        /// <summary>
        ///     <seealso cref="StructTable" />
        /// </summary>
        [FileExtension("struct")]
        StructTable = 0x1612_1900,

        /// <summary>
        ///     S-Archive?
        /// </summary>
        [FileExtension("sarc")]
        SARC = 'S' << 0 | 'A' << 8 | 'R' << 16 | 'C' << 24,

        /// <summary>
        ///     Elixir Archive
        /// </summary>
        [FileExtension("elixir")]
        ElixirArchive = 'E' << 0 | 'A' << 8 | 'R' << 16 | 'C' << 24,

        /// <summary>
        ///     S-Package?
        /// </summary>
        [FileExtension("spkg")]
        SPKG = 'S' << 0 | 'P' << 8 | 'K' << 16 | 'G' << 24,

        /// <summary>
        ///     KTGL Sound Resource
        /// </summary>
        [FileExtension("ktsl2stbin")]
        KTSR = 'K' << 0 | 'T' << 8 | 'S' << 16 | 'R' << 24,

        /// <summary>
        ///     KTGL Sound Container
        /// </summary>
        [FileExtension("ktsl2asbin")]
        KTSC = 'K' << 0 | 'T' << 8 | 'S' << 16 | 'C' << 24,

        /// <summary>
        ///     KTGL Sound Sample
        /// </summary>
        [FileExtension("ktss")]
        KTSS = 'K' << 0 | 'T' << 8 | 'S' << 16 | 'S' << 24,

        /// <summary>
        ///     KTGL Ogg Vorbis Sound
        /// </summary>
        [FileExtension("kvs")]
        KOVS = 'K' << 0 | 'O' << 8 | 'V' << 16 | 'S' << 24,

        /// <summary>
        ///     Ogg Vorbis
        /// </summary>
        [FileExtension("ogg")]
        OGG = 'O' << 0 | 'g' << 8 | 'g' << 16 | 'S' << 24,

        /// <summary>
        ///     Scene
        /// </summary>
        [FileExtension("scene")]
        SCEN = 'S' << 0 | 'C' << 8 | 'E' << 16 | 'N' << 24,

        /// <summary>
        ///     Shader Pack?
        /// </summary>
        [FileExtension("shaderpack")]
        SPK3 = '3' << 0 | 'S' << 8 | 'P' << 16 | 'K' << 24,

        /// <summary>
        ///     Shader Pack?
        /// </summary>
        [FileExtension("swgq")]
        SWGQ = 'S' << 0 | 'W' << 8 | 'G' << 16 | 'Q' << 24,

        /// <summary>
        ///     River?
        /// </summary>
        [FileExtension("river")]
        River = 'R' << 0 | 'I' << 8 | 'V' << 16 | 'E' << 24,

        /// <summary>
        ///     Rig B?
        /// </summary>
        [FileExtension("rig")]
        RIGB = 'R' << 24 | 'I' << 16 | 'G' << 8 | 'B' << 0,

        /// <summary>
        ///     DATD
        /// </summary>
        [FileExtension("datd")]
        DATD = 'D' << 0 | 'A' << 8 | 'T' << 16 | 'D' << 24,

        /// <summary>
        ///     LCD 0
        /// </summary>
        [FileExtension("lcd0")]
        LCD0 = '0' << 24 | 'L' << 16 | 'C' << 8 | 'D' << 0,

        /// <summary>
        ///     WebM Video Container
        /// </summary>
        [FileExtension("webm")]
        WEBM = 0xA3DF_451A,

        /// <summary>
        ///     KTGL Model Container
        /// </summary>
        [FileExtension("mdlk")]
        MDLK = 'M' << 0 | 'D' << 8 | 'L' << 16 | 'K' << 24,

        /// <summary>
        ///     KTGL Screen Layout Texture
        /// </summary>
        [FileExtension("kscl")]
        ScreenLayout = 'K' << 24 | 'S' << 16 | 'C' << 8 | 'L' << 0,

        /// <summary>
        ///     KTGL Screen Layout
        /// </summary>
        [FileExtension("kslt")]
        ScreenLayoutTexture = 'K' << 24 | 'S' << 16 | 'L' << 8 | 'T' << 0,

        /// <summary>
        ///     KTGL Video Source (usually encrypted)
        /// </summary>
        [FileExtension("kslt")]
        VideoSource = 'G' << 24 | '1' << 16 | 'V' << 8 | 'S' << 0,

        /// <summary>
        ///     KTGL Font
        /// </summary>
        [FileExtension("g1n")]
        Font = 'G' << 24 | '1' << 16 | 'L' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL G1L
        /// </summary>
        [FileExtension("g1l")]
        LosslessAudio = 'G' << 24 | '1' << 16 | 'L' << 8 | '_' << 0,

        /// <summary>
        ///     teXt Localization, version 19
        /// </summary>
        [FileExtension("text")]
        TextLocalization19 = 'X' << 0 | 'L' << 8 | 0x13 << 16,

        /// <summary>
        ///     KTGL Texture Group
        /// </summary>
        [FileExtension("g1t")]
        TextureGroup = 'G' << 24 | '1' << 16 | 'T' << 8 | 'G' << 0,

        /// <summary>
        ///     KTGL Model Base
        /// </summary>
        [FileExtension("g1m")]
        Model = 'G' << 24 | '1' << 16 | 'M' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL Animation
        /// </summary>
        [FileExtension("g2a")]
        AnimationV2 = 'G' << 24 | '2' << 16 | 'A' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL Animation
        /// </summary>
        [FileExtension("g1a")]
        Animation = 'G' << 24 | '1' << 16 | 'A' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL Model Group
        /// </summary>
        [FileExtension("g1mg")]
        ModelGroup = 'M' << 0 | 'D' << 8 | 'L' << 16 | 'K' << 24,

        /// <summary>
        ///     KTGL Effect Model
        /// </summary>
        [FileExtension("g1em")]
        EffectManager = 'G' << 24 | '1' << 16 | 'E' << 8 | 'M' << 0,

        /// <summary>
        ///     KTGL Effect
        /// </summary>
        [FileExtension("g1fx")]
        Effect = 'G' << 24 | '1' << 16 | 'F' << 8 | 'X' << 0,

        /// <summary>
        ///     Sound E Bin
        /// </summary>
        [FileExtension("sebin")]
        SEBin = 'S' << 24 | 'L' << 16 | 'O' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL PostFX Shader Bundle
        /// </summary>
        [FileExtension("postfx")]
        PostFX = 'K' << 24 | 'P' << 16 | 'S' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL Model Skeleton
        /// </summary>
        ModelSkeleton = 'G' << 24 | '1' << 16 | 'M' << 8 | 'S' << 0,

        /// <summary>
        ///     KTGL Model Format
        /// </summary>
        ModelFormat = 'G' << 24 | '1' << 16 | 'M' << 8 | 'F' << 0,

        /// <summary>
        ///     KTGL Model Geometry
        /// </summary>
        ModelGeometry = 'G' << 24 | '1' << 16 | 'M' << 8 | 'G' << 0,

        /// <summary>
        ///     KTGL Model Matrices
        /// </summary>
        ModelMatrix = 'G' << 24 | '1' << 16 | 'M' << 8 | 'M' << 0,

        /// <summary>
        ///     KTGL Model ExtraData.
        /// </summary>
        ModelExtra = 'E' << 24 | 'X' << 16 | 'T' << 8 | 'R' << 0,

        /// <summary>
        ///     KTGL Collision Model.
        /// </summary>
        ModelCollision = 'C' << 24 | 'O' << 16 | 'L' << 8 | 'L' << 0,

        /// <summary>
        ///     KTGL Cloth Driver
        /// </summary>
        ModelClothDriver = 'N' << 24 | 'U' << 16 | 'N' << 8 | 'O' << 0,

        /// <summary>
        ///     KTGL Cloth
        /// </summary>
        ModelCloth = 'N' << 24 | 'U' << 16 | 'N' << 8 | 'V' << 0,

        /// <summary>
        ///     KTGL Cloth
        /// </summary>
        ModelClothSurface = 'N' << 24 | 'U' << 16 | 'N' << 8 | 'S' << 0,

        /// <summary>
        ///     KTGL Soft Body voxels
        /// </summary>
        ModelSoftbody = 'S' << 24 | 'O' << 16 | 'F' << 8 | 'T' << 0,

        /// <summary>
        ///     KTGL Hair voxels
        /// </summary>
        ModelHair = 'H' << 24 | 'A' << 16 | 'I' << 8 | 'R' << 0,

        /// <summary>
        ///     Model Pack
        /// </summary>
        [FileExtension(".gmpk")]
        GMPK = 'G' << 0 | 'M' << 8 | 'P' << 16 | 'K' << 24,

        /// <summary>
        ///     Animation Pack
        /// </summary>
        [FileExtension(".gapk")]
        GAPK = 'G' << 0 | 'A' << 8 | 'P' << 16 | 'K' << 24,


        /// <summary>
        ///     Effect Pack
        /// </summary>
        [FileExtension(".gepk")]
        GEPK = 'G' << 0 | 'E' << 8 | 'P' << 16 | 'K' << 24,


        /// <summary>
        /// </summary>
        [FileExtension(".rtrpk")]
        RTRPK = 'R' << 0 | 'T' << 8 | 'P' << 16 | 'K' << 24,

        /// <summary>
        ///     Wave Header Data
        /// </summary>
        [FileExtension(".wave")]
        RIFF = 'R' << 0 | 'I' << 8 | 'F' << 16 | 'F' << 24,

        /// <summary>
        ///     Wave Header Data
        /// </summary>
        [FileExtension(".sed")]
        WHD = 'W' << 0 | 'H' << 8 | 'D' << 16 | '1' << 24,

        /// <summary>
        ///     HDDB
        /// </summary>
        [FileExtension(".hdb")]
        HDDB = 'H' << 0 | 'D' << 8 | 'D' << 16 | 'B' << 24,

        /// <summary>
        ///     Wave Bank Header
        /// </summary>
        [FileExtension("wbh")]
        WBH = 'W' << 24 | 'B' << 16 | 'H' << 8 | '_' << 0,

        /// <summary>
        ///     Wave Bank Data
        /// </summary>
        [FileExtension("wbd")]
        WBD = 'W' << 24 | 'B' << 16 | 'D' << 8 | '_' << 0,

        /// <summary>
        ///     DEFLATE Compressed File.
        /// </summary>
        [FileExtension("gz")]
        Compressed = 0x0001_0000,

        /// <summary>
        ///     DEFLATE Compressed File.
        /// </summary>
        [FileExtension("gz")]
        CompressedChonky = 0x0002_0000
    }
}
