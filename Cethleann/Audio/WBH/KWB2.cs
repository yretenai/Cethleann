using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using Cethleann.Structure.Resource.Audio.WHD;
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
        public List<(KWB2Entry? Header, object[] Streams)> KWBEntries = new List<(KWB2Entry?, object[])>();

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
                    KWBEntries.Add((null, Array.Empty<object>()));
                    continue;
                }

                var kwbHeader = MemoryMarshal.Read<KWB2Entry>(data.Slice(pointers[i]));

                var entry = MemoryMarshal.Read<KWB2Entry>(data.Slice(pointers[i]));
                var offset = 0x2c;
                var size = 0x48;
                if (kwbHeader.Version >= 0xC000)
                {
                    offset = entry.BlockOffset;
                    size = entry.BlockSize;
                }

                var entries = new object[entry.Streams];
                for (var j = 0; j < entry.Streams; ++j)
                {
                    var chunk = data.Slice(pointers[i] + offset, size);
                    var baseHeader = MemoryMarshal.Read<KWB2PCMStream>(chunk);
                    entries[j] = baseHeader.Codec switch
                    {
                        KWB2PCMCodec.PCM16 => baseHeader,
                        KWB2PCMCodec.MSADPCM => MemoryMarshal.Read<KWB2MSADPCMStream>(chunk),
                        KWB2PCMCodec.GCADPCM => MemoryMarshal.Read<KWB2GCADPCMStream>(chunk),
                        _ => baseHeader
                    };
                    offset += entry.BlockSize;
                }

                KWBEntries.Add((entry, entries));
            }
        }

        /// <summary>
        ///     Filename database
        /// </summary>
        public HDDB NameDatabase { get; set; } = new HDDB();

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
        public List<WBHEntry[]> Entries => KWBEntries.Select(x => x.Streams.Select(y =>
        {
            var kwb = y switch
            {
                KWB2PCMStream s => s,
                KWB2MSADPCMStream s => s.Base,
                KWB2GCADPCMStream s => s.Base,
                _ => default
            };

            var setup = default(object);

            if (y is KWB2GCADPCMStream gcadpcmStream) setup = MemoryMarshal.Cast<GCADPCMCoefficient, short>(new Span<GCADPCMCoefficient>(new[] { gcadpcmStream.Coefficient1, gcadpcmStream.Coefficient2 })).ToArray();

            return new WBHEntry
            {
                Offset = kwb.Offset,
                Size = kwb.Size,
                Samples = kwb.SampleCount,
                Codec = kwb.Codec switch
                {
                    KWB2PCMCodec.MSADPCM => WAVECodec.MSADPCM,
                    KWB2PCMCodec.PCM16 => WAVECodec.PCM,
                    KWB2PCMCodec.GCADPCM => WAVECodec.GCADPCM,
                    _ => WAVECodec.PCM
                },
                Frequency = kwb.SampleRate,
                BlockAlign = kwb.FrameSize,
                Channels = kwb.Channels,
                Setup = setup
            };
        }).ToArray()).ToList();

        /// <inheritdoc />
        public List<string?> Names => NameDatabase.Entries.Select(x => x.ElementAtOrDefault(NameIndex) ?? x.ElementAtOrDefault(SecondaryNameIndex) ?? x.FirstOrDefault()).ToList();
    }
}
