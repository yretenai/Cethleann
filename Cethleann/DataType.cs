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
        StructTable = 0x16121900,
        /// <summary>
        /// S-Archive?
        /// </summary>
        SARC = 0x43524153,
        /// <summary>
        /// S-Package?
        /// </summary>
        SPKG = 0x474B5053,
        /// <summary>
        /// Koei Texture
        /// </summary>
        GT = 0x47325447,
        /// <summary>
        /// Koei Tecmo Sound Resource
        /// </summary>
        KTSR = 0x5253544B,
        /// <summary>
        /// Scene
        /// </summary>
        SCEN = 0x4E454353,
        /// <summary>
        /// WebM Video Container
        /// </summary>
        WEBM = 0xA3DF451A
    }
}