using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Cethleann.Structure.DataStructs.DissidiaOO
{
    [PublicAPI]
    public struct AddParamTarget
    {
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        [StringLength(0x40)]
        public string Start { get; set; }
        [StringLength(0x40)]
        public string End { get; set; }
        public int Unknown5 { get; set; }
        public int Unknown6 { get; set; }
    }
}
