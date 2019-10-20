using Cethleann.G1.G1ModelSection;
using Cethleann.Structure.Art;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cethleann.G1
{
    public class G1Model
    {
        private readonly static int SupportedBaseVersion = 37;
        private readonly static int SupportedFVersion = 32;
        private readonly static int SupportedSVersion = 29;
        private readonly static int SupportedMVersion = 20;
        private readonly static int SupportedGVersion = 44;
        private readonly static int SupportedExtraVersion = 10;

        public List<IG1MSection> Sections { get; } = new List<IG1MSection>();

        public G1Model(Span<byte> data)
        {

        }
    }
}
