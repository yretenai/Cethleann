using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Structure
{
    /// <summary>
    ///     Magic Values for most files
    /// </summary>
    [PublicAPI]
    public enum DataType : uint
    {
        /// <summary>
        ///     Used as a dummy
        /// </summary>
        [FileExtension("bin")]
        None = 0,

        /// <summary>
        ///     LINKDATA found in AoT2.
        /// </summary>
        LINKDATA = 0x00077DF9,

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
        ElixirArchive = 'E' << 24 | 'A' << 16 | 'R' << 8 | 'C' << 0,

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
        ///     Swing Quantized Data
        /// </summary>
        [FileExtension("swg")]
        SwingDefinition = 'S' << 0 | 'W' << 8 | 'G' << 16 | 'Q' << 24,

        /// <summary>
        ///     River?
        /// </summary>
        [FileExtension("river")]
        River = 'R' << 0 | 'I' << 8 | 'V' << 16 | 'E' << 24,

        /// <summary>
        ///     Rig B?
        /// </summary>
        [FileExtension("rig")]
        RIGB = 'R' << 0 | 'I' << 8 | 'G' << 16 | 'B' << 24,

        /// <summary>
        ///     the fuck?
        /// </summary>
        [FileExtension("ertr")]
        ERTR = 'E' << 24 | 'R' << 16 | 'T' << 8 | 'R' << 0,

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
        ///     KTGL Model Pack
        /// </summary>
        [FileExtension("mdlpack")]
        ModelPack = 'M' << 0 | 'D' << 8 | 'L' << 16 | 'R' << 24, // MDLRESPK

        /// <summary>
        ///     KTGL Model Pack
        /// </summary>
        [FileExtension("mdltexpack")]
        ModelTexPack = 'M' << 0 | 'D' << 8 | 'L' << 16 | 'T' << 24, // MDLTEXPK

        /// <summary>
        ///     KTGL EXARG
        /// </summary>
        [FileExtension("exarg")]
        ExtraArg = 'E' << 0 | 'X' << 8 | 'A' << 16 | 'R' << 24, // EXARG000

        /// <summary>
        ///     KTGL Effect Pack
        /// </summary>
        [FileExtension("effectpack")]
        EffectPack = 'E' << 0 | 'F' << 8 | 'F' << 16 | 'R' << 24, // EFFRESPK

        /// <summary>
        ///     KTGL Animation Pack
        /// </summary>
        [FileExtension("g1apack")]
        G2APack = 'G' << 0 | '2' << 8 | 'A' << 16 | '_' << 24, // G2A_PACK

        /// <summary>
        ///     KTGL Animation Pack
        /// </summary>
        [FileExtension("g1epack")]
        G1EPack = 'G' << 0 | '1' << 8 | 'E' << 16 | '_' << 24, // G1E_PACK

        /// <summary>
        ///     KTGL Animation Pack
        /// </summary>
        [FileExtension("g1mpack")]
        G1MPack = 'G' << 0 | '1' << 8 | 'M' << 16 | '_' << 24, // G1M_PACK

        /// <summary>
        ///     KTGL Animation Pack
        /// </summary>
        [FileExtension("g1copack")]
        G1COPack = 'G' << 0 | '1' << 8 | 'C' << 16 | 'O' << 24, // G1COPACK

        /// <summary>
        ///     KTGL KTFK Pack
        /// </summary>
        [FileExtension("ktfkpack")]
        KTFKPack = 'T' << 0 | 'R' << 8 | 'R' << 16 | 'R' << 24, // TRRRESPK

        /// <summary>
        ///     KTGL G1CO Pack
        /// </summary>
        [FileExtension("colpack")]
        CollisionPack = 'C' << 0 | 'O' << 8 | 'L' << 16 | 'R' << 24, // COLRESPK

        /// <summary>
        ///     KTGL TD Pack
        /// </summary>
        [FileExtension("tdpack")]
        TDPack = 'T' << 0 | 'D' << 8 | 'P' << 16 | 'A' << 24, // TDPACK

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
        ///     KTGL Wrapped File
        /// </summary>
        [FileExtension("g1l")]
        Lazy = 'G' << 24 | '1' << 16 | 'L' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL Font
        /// </summary>
        [FileExtension("g1n")]
        Font2 = 'G' << 24 | '1' << 16 | 'N' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL Font
        /// </summary>
        [FileExtension("g1h")]
        Morph = 'G' << 24 | '1' << 16 | 'H' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL Collision mesh
        /// </summary>
        [FileExtension("g1c")]
        Collision = 'G' << 24 | '1' << 16 | 'C' << 8 | 'O' << 0,

        /// <summary>
        ///     XL, version 19
        /// </summary>
        [FileExtension("xl")]
        XL = 'X' << 0 | 'L' << 8 | 0x13 << 16,

        /// <summary>
        ///     XL, version 20
        /// </summary>
        [FileExtension("xlstruct")]
        XL20 = 'X' << 0 | 'L' << 8 | 0x14 << 16,

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
        [FileExtension("g1a")]
        AnimationV2 = 'G' << 24 | '2' << 16 | 'A' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL Animation
        /// </summary>
        [FileExtension("g1a")]
        Animation = 'G' << 24 | '1' << 16 | 'A' << 8 | '_' << 0,

        /// <summary>
        ///     KTGL Shader
        /// </summary>
        [FileExtension("g1s")]
        Shader = 'G' << 24 | '2' << 16 | 'S' << 8 | '_' << 0,

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
        G1MS = 'G' << 24 | '1' << 16 | 'M' << 8 | 'S' << 0,

        /// <summary>
        ///     KTGL Model Format
        /// </summary>
        G1MF = 'G' << 24 | '1' << 16 | 'M' << 8 | 'F' << 0,

        /// <summary>
        ///     KTGL Model Geometry
        /// </summary>
        G1MG = 'G' << 24 | '1' << 16 | 'M' << 8 | 'G' << 0,

        /// <summary>
        ///     KTGL Model Matrices
        /// </summary>
        G1MM = 'G' << 24 | '1' << 16 | 'M' << 8 | 'M' << 0,

        /// <summary>
        ///     KTGL Model ExtraData.
        /// </summary>
        [FileExtension("extra")]
        EXTR = 'E' << 24 | 'X' << 16 | 'T' << 8 | 'R' << 0,

        /// <summary>
        ///     KTGL Collision Model.
        /// </summary>
        COLL = 'C' << 24 | 'O' << 16 | 'L' << 8 | 'L' << 0,

        /// <summary>
        ///     KTGL Cloth Driver
        /// </summary>
        NUNO = 'N' << 24 | 'U' << 16 | 'N' << 8 | 'O' << 0,

        /// <summary>
        ///     KTGL Cloth Vividly
        /// </summary>
        NUNV = 'N' << 24 | 'U' << 16 | 'N' << 8 | 'V' << 0,

        /// <summary>
        ///     KTGL Cloth Softly
        /// </summary>
        NUNS = 'N' << 24 | 'U' << 16 | 'N' << 8 | 'S' << 0,

        /// <summary>
        ///     KTGL Soft Body voxels
        /// </summary>
        SOFT = 'S' << 24 | 'O' << 16 | 'F' << 8 | 'T' << 0,

        /// <summary>
        ///     KTGL Hair voxels
        /// </summary>
        HAIR = 'H' << 24 | 'A' << 16 | 'I' << 8 | 'R' << 0,

        /// <summary>
        ///     Model Pack
        /// </summary>
        [FileExtension("gmpk")]
        GMPK = 'G' << 0 | 'M' << 8 | 'P' << 16 | 'K' << 24,

        /// <summary>
        ///     Model Pack
        /// </summary>
        [FileExtension("lmpk")]
        LMPK = 'L' << 0 | 'M' << 8 | 'P' << 16 | 'K' << 24,

        /// <summary>
        ///     Animation Pack
        /// </summary>
        [FileExtension("gapk")]
        GAPK = 'G' << 0 | 'A' << 8 | 'P' << 16 | 'K' << 24,


        /// <summary>
        ///     Effect Pack
        /// </summary>
        [FileExtension("gepk")]
        GEPK = 'G' << 0 | 'E' << 8 | 'P' << 16 | 'K' << 24,


        /// <summary>
        /// </summary>
        [FileExtension("rtrpk")]
        RTRPK = 'R' << 0 | 'T' << 8 | 'P' << 16 | 'K' << 24,

        /// <summary>
        ///     Wave Header Data
        /// </summary>
        [FileExtension("wave")]
        RIFF = 'R' << 0 | 'I' << 8 | 'F' << 16 | 'F' << 24,

        /// <summary>
        ///     Wave Header Data
        /// </summary>
        [FileExtension("sed")]
        WHD = 'W' << 0 | 'H' << 8 | 'D' << 16 | '1' << 24,

        /// <summary>
        ///     HDDB
        /// </summary>
        [FileExtension("hdb")]
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
        ///     Koei Readable Database File
        /// </summary>
        [FileExtension("rdb")]
        RDB = 'K' << 24 | 'R' << 16 | 'D' << 8 | '_' << 0,

        /// <summary>
        ///     Koei Readable Database Index
        /// </summary>
        [FileExtension("rdb.bin")]
        RDBIndex = 'K' << 24 | 'R' << 16 | 'D' << 8 | 'I' << 0,

        /// <summary>
        ///     Koei Name Database File
        /// </summary>
        [FileExtension("name")]
        NDB = 'K' << 24 | 'N' << 16 | 'R' << 8 | '_' << 0,

        /// <summary>
        ///     Koei Name Database Index
        /// </summary>
        [FileExtension("name.bin")]
        NDBIndex = 'K' << 24 | 'N' << 16 | 'R' << 8 | 'I' << 0,

        /// <summary>
        ///     Koei Object Database File
        /// </summary>
        [FileExtension("kidsobjdb")]
        OBJDB = 'K' << 24 | 'O' << 16 | 'D' << 8 | '_' << 0,

        /// <summary>
        ///     Koei Object Database Index
        /// </summary>
        [FileExtension("kidsobjdb.bin")]
        OBJDBIndex = 'K' << 24 | 'O' << 16 | 'D' << 8 | 'I' << 0,

        /// <summary>
        ///     Koei Object Database Record
        /// </summary>
        [FileExtension("kidsobjdb.bin")]
        OBJDBRecord = 'K' << 24 | 'O' << 16 | 'D' << 8 | 'R' << 0,

        /// <summary>
        ///     DEFLATE Compressed File.
        /// </summary>
        [FileExtension("gz")]
        Compressed = 0x0001_0000,

        /// <summary>
        ///     DEFLATE Compressed File.
        /// </summary>
        [FileExtension("gz")]
        CompressedChonky = 0x0002_0000,

        /// <summary>
        ///     TN PKGINFO
        /// </summary>
        [FileExtension("pkginfo")]
        PackageInfo = 'p' << 0 | 'k' << 8 | 'g' << 16 | 'i' << 24, // pkginfo

        [FileExtension("wmv")]
        WMV = 0x75B22630,

        /// <summary>
        ///     DDS, can't fcking believe it.
        /// </summary>
        [FileExtension("dds")]
        DDS = 'D' << 0 | 'D' << 8 | 'S' << 16 | ' ' << 24,

        /// <summary>
        ///     CHAR_DAT
        /// </summary>
        [FileExtension("chardata")]
        CharacterData = 'c' << 0 | 'h' << 8 | 'a' << 16 | 'r' << 24, // char_dat

        /// <summary>
        ///     Cont?
        /// </summary>
        [FileExtension("cont")]
        CONT = 'C' << 0 | 'O' << 8 | 'N' << 16 | 'T' << 24,

        /// <summary>
        ///     Team ninja Model Container?
        /// </summary>
        [FileExtension("tmc")]
        TMC = 'T' << 0 | 'M' << 8 | 'C' << 16,

        /// <summary>
        ///     clip
        /// </summary>
        [FileExtension("clip")]
        Clip = 'c' << 0 | 'l' << 8 | 'i' << 16 | 'p' << 24,

        /// <summary>
        ///     bodybase
        /// </summary>
        [FileExtension("bodybase")]
        Body = 'b' << 0 | 'o' << 8 | 'd' << 16 | 'y' << 24, // bodybase

        /// <summary>
        ///     pbsmat
        /// </summary>
        [FileExtension("material")]
        PBSMaterial = 'P' << 0 | 'B' << 8 | 'S' << 16 | 'M' << 24, // PDBMat

        /// <summary>
        ///     TDPack
        /// </summary>
        [FileExtension("tdpack")]
        OldTDPack = 't' << 0 | 'd' << 8 | 'p' << 16 | 'a' << 24 // tdpack
    }
}
