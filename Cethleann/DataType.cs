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
        StructTable = 0x16121900,
        /// <summary>
        /// S-Archive?
        /// </summary>
        [FileExtension("sarc")]
        SARC = 0x43524153,
        /// <summary>
        /// S-Package?
        /// </summary>
        [FileExtension("spkg")]
        SPKG = 0x474B5053,
        /// <summary>
        /// Koei Texture
        /// </summary>
        [FileExtension("g1t")]
        GT = 0x47315447,
        /// <summary>
        /// Koei Tecmo Sound Resource
        /// </summary>
        [FileExtension("ktsl2stbin")]
        KTSR = 0x5253544B,
        /// <summary>
        /// Scene
        /// </summary>
        [FileExtension("scene")]
        SCEN = 0x4E454353,
        /// <summary>
        /// WebM Video Container
        /// </summary>
        [FileExtension("webm")]
        WEBM = 0xA3DF451A,
        /// <summary>
        /// teXt Localization, version 19.
        /// </summary>
        [FileExtension("kt19xl")]
        XL_19 = 0x00134C58
    }
}