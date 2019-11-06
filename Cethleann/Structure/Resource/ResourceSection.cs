namespace Cethleann.Structure.Resource
{
    /// <summary>
    ///     Resources found in G1/2 assets
    /// </summary>
    public enum ResourceSection
    {
        /// <summary>
        ///     Koei Model Skeleton
        /// </summary>
        ModelSkeleton = 'G' << 24 | '1' << 16 | 'M' << 8 | 'S' << 0,

        /// <summary>
        ///     Koei Model F?
        /// </summary>
        ModelF = 'G' << 24 | '1' << 16 | 'M' << 8 | 'F' << 0,

        /// <summary>
        ///     Koei Model Geometry
        /// </summary>
        ModelGeometry = 'G' << 24 | '1' << 16 | 'M' << 8 | 'G' << 0,

        /// <summary>
        ///     Koei Model Matrices
        /// </summary>
        ModelMatrix = 'G' << 24 | '1' << 16 | 'M' << 8 | 'M' << 0,

        /// <summary>
        ///     Koei Model ExtraData.
        /// </summary>
        ModelExtra = 'E' << 24 | 'X' << 16 | 'T' << 8 | 'R' << 0,

        /// <summary>
        ///     Koei Collision Model.
        /// </summary>
        ModelCollision = 'C' << 24 | 'O' << 16 | 'L' << 8 | 'L' << 0,
                         
        /// <summary>
        ///     Assumption.
        /// </summary>
        ModelCloth = 'N' << 24 | 'U' << 16 | 'N' << 8 | 'O' << 0
    }
}
