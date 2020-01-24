using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.WHD;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Audio.WBH
{
    /// <summary>
    ///     KWB2 MSADPCM Soundbank
    /// </summary>
    [PublicAPI]
    public class KWB2 : IWBHSoundbank
    {
        /// <summary>
        ///     KWB2 Entries
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public List<(KWB2Entry Header, KWB2PCMStream[] Streams)> KWBEntries = new List<(KWB2Entry, KWB2PCMStream[])>();

        /// <summary>
        ///     Initialize with buffer data.
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

            if (Header.HDDBPointer > 0) NameDatabase = new HDDB(data.Slice(Header.HDDBPointer), Header.Count);

            var pointers = MemoryMarshal.Cast<byte, int>(data.Slice(SizeHelper.SizeOf<KWB2Header>(), 4 * Header.Count));
            for (var i = 0; i < Header.Count; ++i)
            {
                if (pointers[i] == 0)
                {
                    KWBEntries.Add((default, new KWB2PCMStream[0]));
                    continue;
                }

                var kwbHeader = MemoryMarshal.Read<KWB2EntryHeader>(data.Slice(pointers[i]));
                if (kwbHeader.Codec != KWB2Codec.WAVE)
                {
                    KWBEntries.Add((default, new KWB2PCMStream[0]));
                    continue;
                }

                var entry = MemoryMarshal.Read<KWB2Entry>(data.Slice(pointers[i]));
                var streams = MemoryMarshal.Cast<byte, KWB2PCMStream>(data.Slice(pointers[i] + entry.BlockOffset, entry.BlockSize * entry.Header.Streams)).ToArray();
                KWBEntries.Add((entry, streams));
            }
        }

        /// <summary>
        ///     Filename database
        /// </summary>
        public HDDB NameDatabase { get; set; }

        /// <summary>
        ///     Underlying header
        /// </summary>
        public KWB2Header Header { get; set; }

        /// <summary>
        ///     Used to lookup names
        /// </summary>
        public int NameIndex { get; set; }

        /// <summary>
        ///     Used to lookup names
        /// </summary>
        public int SecondaryNameIndex { get; set; } = 1;

        /// <inheritdoc />
        public List<WBHEntry[]> Entries => KWBEntries.Select(x => x.Streams.Select(y => new WBHEntry
        {
            Offset = y.Offset,
            Size = y.Size,
            Samples = y.SampleCount,
            Codec = y.Codec switch
            {
                KWB2PCMCodec.MSADPCM => WBHCodec.MSADPCM,
                KWB2PCMCodec.PCM16 => WBHCodec.PCM,
                _ => WBHCodec.MSADPCM
            },
            Frequency = y.SampleRate,
            BlockAlign = y.FrameSize
        }).ToArray()).ToList();

        /// <inheritdoc />
        public List<string> Names => NameDatabase?.Entries?.Select(x => x.ElementAtOrDefault(NameIndex) ?? x.ElementAtOrDefault(SecondaryNameIndex) ?? x.FirstOrDefault(y => y != null)).ToList();
    }
}
