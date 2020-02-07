using Cethleann.Koei;
using JetBrains.Annotations;

namespace Softness.Hash
{
    [PublicAPI]
    public static class Program
    {
        private static void Main(string[] args)
        {
            RDB.Hash("Masq_title_c01", "G1T");
        }
    }
}
