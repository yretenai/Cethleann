using Cethleann.Structure;
using Cethleann.Structure.KTID;
using DragonLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Cethleann.Archive
{
    public class PRDB : IDisposable
    {
        public PRDB(Stream data)
        {
            DataStream = data;
            var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<PRDBHeader>()]);
            DataStream.Read(buffer);
            Header = MemoryMarshal.Read<PRDBHeader>(buffer);
            Entries = new List<(PRDBEntry, long)>();
            buffer = new Span<byte>(new byte[SizeHelper.SizeOf<PRDBEntry>()]);
            while (true)
            {
                var p = DataStream.Position;
                DataStream.Read(buffer);
                var entry = MemoryMarshal.Read<PRDBEntry>(buffer);
                if (entry.Magic != DataType.RDBIndex) // wtf?
                {
                    p = DataStream.Position = p.Align(0x10000);
                    DataStream.Read(buffer);
                    entry = MemoryMarshal.Read<PRDBEntry>(buffer);
                }

                Entries.Add((entry, p));
                DataStream.Position = p + entry.EntrySize;
                if (DataStream.Length - DataStream.Position < buffer.Length) break;
            }
        }

        public Stream DataStream { get; set; }

        public PRDBHeader Header { get; set; }
        public List<(PRDBEntry entry, long position)> Entries { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PRDB() => Dispose(true);


        public Span<byte> ReadEntry(long pos)
        {
            var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<PRDBEntry>()]);
            DataStream.Position = pos;
            DataStream.Read(buffer);
            var entry = MemoryMarshal.Read<PRDBEntry>(buffer);
            buffer = new byte[entry.Size];
            DataStream.Position += entry.EntrySize - entry.ContentSize;
            DataStream.Read(buffer);
            return buffer;
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            DataStream.Dispose();
        }
    }
}
