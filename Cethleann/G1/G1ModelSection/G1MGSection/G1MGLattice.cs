using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     "Lattice" is an assumption.
    ///     Just a list of float data, used for bones maybe?
    /// </summary>
    /// <inheritdoc />
    public class G1MGLattice : IG1MGSection
    {
        internal G1MGLattice(Span<byte> data, ModelGeometrySection sectionInfo)
        {
            Section = sectionInfo;
            Lattice = MemoryMarshal.Cast<byte, ModelGeometryLattice>(data).ToArray();
        }

        /// <summary>
        ///     "Lattice" is an assumption.
        /// </summary>
        public ModelGeometryLattice[] Lattice { get; }

        /// <inheritdoc />
        public ModelGeometrySection Section { get; }

        /// <inheritdoc />
        public ModelGeometryType Type => ModelGeometryType.Lattice;
    }
}
