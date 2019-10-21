using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Model;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    public class G1MGLattice : IG1MGSection
    {
        public G1MGLattice(Span<byte> data, ModelGeometrySection sectionInfo)
        {
            Section = sectionInfo;
            Lattice = MemoryMarshal.Cast<byte, ModelGeometryLattice>(data).ToArray();
        }

        public ModelGeometryLattice[] Lattice { get; }
        public ModelGeometrySection Section { get; }
        public ModelGeometryType Type => ModelGeometryType.Lattice;
    }
}
