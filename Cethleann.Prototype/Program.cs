using System.IO;
using JetBrains.Annotations;

namespace Cethleann.Prototype
{
    static class Program
    {
        [PublicAPI]
        static void Main(string[] args)
        {
#if DEBUG
            // :-)
            var oid = new OId(File.ReadAllBytes(@"PC00A_MODELOid.bin"));
            File.WriteAllLines(@"PC00A_MODELOid.txt", oid.Names);
#endif
        }
    }
}
