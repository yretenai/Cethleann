using Cethleann.Structure.Resource.Model;
using System;
using System.Runtime.InteropServices;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    public class G1MGLattice : IG1MGSection
    {
        public ModelGeometrySection Section { get; }
        public ModelGeometryType Type => ModelGeometryType.Lattice;
        public ModelGeometryLattice[] Lattice { get; }

        public G1MGLattice(Span<byte> data, ModelGeometrySection sectionInfo)
        {
            Section = sectionInfo;
            Lattice = MemoryMarshal.Cast<byte, ModelGeometryLattice>(data).ToArray();
        }
    }
}
