using System;
using System.IO;
using JetBrains.Annotations;

namespace Cethleann.Koei
{
    /// <summary>
    /// First seen in DoA6, but it's used across the company.
    /// It looks like it's the go-to format for KTGL Version 2 / "NewSoftEngine"
    /// </summary>
    [PublicAPI]
    public class RDB : IDisposable
    {
        public Stream EmbeddedBin { get; set; }
        public string Directory { get; set; }
        public string Name { get; set; }

        public RDB(Span<byte> rdb, string name, string directory)
        {
            
        }

        ~RDB()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            EmbeddedBin.Dispose();
        }
    }
}
