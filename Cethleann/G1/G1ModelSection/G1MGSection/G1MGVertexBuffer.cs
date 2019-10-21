using System;
using System.Collections.Generic;
using System.Text;
using Cethleann.Structure.Resource.Model;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    public class G1MGVertexBuffer : IG1MGSection
    {
        public ModelGeometryType Type => ModelGeometryType.VertexBuffer;

        public ModelGeometrySection Section { get; }
    }
}
