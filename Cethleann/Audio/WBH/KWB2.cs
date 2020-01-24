using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.WHD;
using DragonLib;
using DragonLib.IO;

namespace Cethleann.Audio.WBH
{
    /// <summary>
    /// KWB2 MSADPCM Soundbank
    /// </summary>
    public class KWB2 : IWBHSoundbank
    {
        /// <summary>
        /// KWB2 Entries
        /// </summary>
        public List<(KWB2Entry Header, KWB2Stream[] Streams)> KWBEntries = new List<(KWB2Entry, KWB2Stream[])>();

        /// <summary>
        /// Initialize with buffer data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="alternateNames"></param>
        public KWB2(Span<byte> data, bool alternateNames)
        {
            if (alternateNames)
            {
                NameIndex = 1;
                SecondaryNameIndex = 0;
            }

            Header = MemoryMarshal.Read<KWB2Header>(data);
            var pointers = MemoryMarshal.Cast<byte, int>(data.Slice(SizeHelper.SizeOf<KWB2Header>(), 4 * Header.Count));
            for (var i = 0; i < Header.Count; ++i)
            {
                if (pointers[i] == 0) continue;
                var entry = MemoryMarshal.Read<KWB2Entry>(data.Slice(pointers[i]));
                var streams = MemoryMarshal.Cast<byte, KWB2Stream>(data.Slice(pointers[i] + SizeHelper.SizeOf<KWB2Entry>(), entry.Count * SizeHelper.SizeOf<KWB2Stream>())).ToArray();
                Logger.Assert(streams.All(x => x.Unknown2 == 0x1), "streams.All(x => x.Unknown2 == 0x1)", pointers[i].ToString("X"));
                Logger.Assert(streams.All(x => x.FrameSize == 0x16), "streams.All(x=> x.FrameSize == 0x16)", pointers[i].ToString("X"));
                KWBEntries.Add((entry, streams));
            }

            if (Header.HDDBPointer > 0) NameDatabase = new HDDB(data.Slice(Header.HDDBPointer), Header.Count);
        }

        /// <summary>
        /// Filename database
        /// </summary>
        public HDDB NameDatabase { get; set; }
        
        /// <summary>
        /// Underlying header
        /// </summary>
        public KWB2Header Header { get; set; }
        
        /// <summary>
        /// Used to lookup names
        /// </summary>
        public int NameIndex { get; set; }
        
        /// <summary>
        /// Used to lookup names
        /// </summary>
        public int SecondaryNameIndex { get; set; } = 1;

        /// <inheritdoc />
        public List<WBHEntry[]> Entries => KWBEntries.Select(x => x.Streams.Select(y => new WBHEntry
        {
            Offset = y.Offset,
            Size = y.Size,
            Samples = y.SampleCount,
            Codec = WBHCodec.MSADPCM,
            Frequency = y.SampleRate,
            BlockAlign = y.FrameSize
        }).ToArray()).ToList();

        /// <inheritdoc />
        public List<string> Names => NameDatabase?.Entries?.Select(x => x.ElementAtOrDefault(NameIndex) ?? x.ElementAtOrDefault(SecondaryNameIndex) ?? x.FirstOrDefault(y => y != null)).ToList();
    }
}
