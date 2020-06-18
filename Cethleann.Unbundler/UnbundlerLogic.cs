using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Audio;
using Cethleann.Graphics;
using Cethleann.Pack;
using Cethleann.Structure;
using Cethleann.Tables;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cethleann.Unbundler
{
    [PublicAPI]
    public static class UnbundlerLogic
    {
        public static void TryExtractBlobs(string pathBase, List<Memory<byte>> blobs, bool allTypes, List<string?>? names, bool singleFile, bool useDirnameAsName, bool forceExtension, string? extension, UnbundlerFlags flags)
        {
            for (var index = 0; index < blobs.Count; index++)
            {
                var datablob = blobs[index];
                var name = $"{index:X4}";
                var foundName = names?.ElementAtOrDefault(index);
                var ext = $".{extension ?? GetExtension(datablob.Span)}";
                if (foundName != null)
                {
                    name = foundName.SanitizeFilename();
                    ext = !string.IsNullOrWhiteSpace(Path.GetExtension(name)) && !forceExtension ? string.Empty : ext;
                    if (File.Exists($@"{pathBase}\{name}{ext}"))
                    {
                        var oname = name + "_";
                        var i = 1;
                        while (File.Exists($@"{pathBase}\{name}{ext}")) name = oname + $"{i++:X}";
                    }
                }

                var path = $@"{pathBase}\{name}{ext}";
                if (singleFile && blobs.Count == 1)
                {
                    if (useDirnameAsName)
                        path = pathBase + $".{ext}";
                    else
                        path = Path.Combine(Path.GetDirectoryName(pathBase) ?? string.Empty, $"{name}{ext}");
                }

                TryExtractBlob(path, datablob.Span, allTypes, flags, false);
            }
        }

        public static int TryExtractBlob(string blobBase, Span<byte> datablob, bool allTypes, UnbundlerFlags flags, bool skipUnknown)
        {
            if (datablob.Length == 0 && !flags.WriteZero)
            {
                Logger.Warn("Cethleann", $"{blobBase} is zero!");
                return 0;
            }

            var dataType = datablob.GetDataType();

            if (allTypes || flags.Recursive && flags.Depth > 0)
            {
                var recursionLevel = flags.Depth--;
                try
                {
                    if (!datablob.IsKnown())
                    {
                        if (datablob.IsDataTable())
                            if (TryExtractDataTable(blobBase, datablob, flags))
                                return 1;
                        if (datablob.IsBundle())
                            if (TryExtractBundle(blobBase, datablob, flags))
                                return 1;
                        if (datablob.IsSliceBundle())
                            if (TryExtractSliceBundle(blobBase, datablob, flags))
                                return 1;
                        if (datablob.IsPointerBundle())
                            if (TryExtractPointerBundle(blobBase, datablob, flags))
                                return 1;
                        if (datablob.IsByteBundle())
                            if (TryExtractByteBundle(blobBase, datablob, flags))
                                return 1;
                    }

                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (dataType)
                    {
                        case DataType.SCEN when TryExtractSCEN(blobBase, datablob, flags):
                        case DataType.MDLK when TryExtractMDLK(blobBase, datablob, flags):
                        case DataType.KTSR when TryExtractKTSR(blobBase, datablob, flags):
                        case DataType.KTSC when TryExtractKTSC(blobBase, datablob, flags):
                        case DataType.Model when !flags.Recursive && TryExtractG1M(blobBase, datablob, flags):
                        case DataType.XL when TryExtractXL(blobBase, datablob):
                        case DataType.GAPK when TryExtractGAPK(blobBase, datablob, flags, false):
                        case DataType.GEPK when TryExtractGAPK(blobBase, datablob, flags, true):
                        case DataType.GMPK when TryExtractGMPK(blobBase, datablob, flags):
                        case DataType.Lazy when TryExtractG1L(blobBase, datablob, flags):
                        case DataType.KOVS when TryExtractKOVS(blobBase, datablob, flags):
                        case DataType.ElixirArchive when TryExtractElixir(blobBase, datablob, flags):
                        case DataType.RTRPK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.EFFRESPK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.TDPACK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.COLRESPK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.MDLRESPK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.MDLTEXPK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.EXARG when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.TRRRESPK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.G1E_PACK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.G1M_PACK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.G1H_PACK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.G2A_PACK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.HEADPACK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.G1COPACK when TryExtractRESPACK(blobBase, datablob, flags):
                        case DataType.WHD when TryExtractWHD(blobBase, datablob, flags):
                            return 1;
                    }
                }
                finally
                {
                    flags.Depth = recursionLevel;
                }
            }

            if (skipUnknown) return 0;

            var basedir = Path.GetDirectoryName(blobBase);
            if (!Directory.Exists(basedir))
            {
                if (File.Exists(basedir))
                {
                    Logger.Warn("Cethleann", $@"Trying to make a directory named {basedir} but it is a file?");
                    return 0;
                }

                Directory.CreateDirectory(basedir);
            }

            if (File.Exists(blobBase))
            {
                if (flags.KeepBoth)
                {
                    var filename = Path.GetFileNameWithoutExtension(blobBase);
                    var ext = Path.GetExtension(blobBase);
                    var i = 1;
                    while (File.Exists(Path.Combine(basedir ?? string.Empty, $"{filename}_{i}{ext}"))) i += 1;

                    blobBase = Path.Combine(basedir ?? string.Empty, $"{filename}_{i}{ext}");
                }
                else if (flags.Overwrite)
                {
                    File.Delete(blobBase);
                }
                else
                {
                    Logger.Warn("Cethleann", $@"{blobBase} already exists!");
                    return 0;
                }
            }

            Logger.Info("Cethleann", blobBase);
            File.WriteAllBytes(blobBase, datablob.ToArray());

            return 2;
        }

        private static bool TryExtractGAPK(string pathBase, Span<byte> data, UnbundlerFlags flags, bool prependNames)
        {
            try
            {
                var blobs = new GAPK(data);
                if (blobs.Blobs.Count == 0) return true;

                var names = blobs.NameMap.Names;
                if (prependNames)
                {
                    names.Insert(0, blobs.NameMap.Name ?? Path.GetFileName(pathBase));
                    names.Insert(0, blobs.NameMap.Name ?? Path.GetFileName(pathBase));
                }

                TryExtractBlobs(pathBase, blobs.Blobs, false, names, false, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("GAPK", "Failed unpacking GAPK", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractRESPACK(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new RESPACK(data);
                if (blobs.Entries.Count == 0) return true;
                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("RESPACK", "Failed unpacking RTRPK", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractWHD(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new KoeiWaveBank(data, flags.WBHAlternateNames);
                if (blobs.WBH.Soundbank == null || blobs.WBH.Soundbank.Entries.Count == 0) return true;

                var names = blobs.WBH.Soundbank.Names;
                for (var index = 0; index < blobs.WBH.Soundbank.Entries.Count; index++)
                {
                    var streams = blobs.WBH.Soundbank.Entries[index];
                    for (var streamIndex = 0; streamIndex < streams.Length; streamIndex++)
                    {
                        var stream = streams[streamIndex];
                        var wav = blobs.WBD.ReconstructWave(stream, !flags.SkipADPCM);
                        var name = $@"{pathBase}\{names.ElementAtOrDefault(index)?.SanitizeDirname() ?? index.ToString("X8")}".Trim();
                        if (streams.Length > 1) name += $@"\{streamIndex:X8}";
                        TryExtractBlob($@"{name}.wav", wav.Span, false, flags, false);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("WHD", "Failed unpacking WHD", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractGMPK(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new GMPK(data);
                if (blobs.Blobs.Count == 0) return true;

                var nameIndex = 0;
                var names = new List<string?>();
                foreach (var blob in blobs.Blobs)
                {
                    // there probably is a smart way of doing this
                    // but this works 9 out of 10 times.
                    if (blob.Span.GetDataType() == DataType.TextureGroup && nameIndex > 0) nameIndex--;
                    if (blob.Span.GetDataType() == DataType.SwingDefinition && nameIndex > 0) nameIndex--;
                    if (nameIndex >= blobs.NameMap.Names.Count) break;
                    names.Add(nameIndex >= 0 ? blobs.NameMap.Names[nameIndex] : null);
                    nameIndex++;
                }

                TryExtractBlobs(pathBase, blobs.Blobs, false, names, false, false, true, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("GMPK", "Failed unpacking GMPK", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractElixir(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var elixir = new Elixir(data);
                if (elixir.Blobs.Count == 0) return true;
                TryExtractBlobs(pathBase, elixir.Blobs, false, elixir.Entries.Select(x => x.filename).ToList(), true, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("ELIXIR", "Failed unpacking ELIXIR", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        public static string GetExtension(Span<byte> data)
        {
            if (data.IsKnown()) return data.GetDataType().GetExtension();
            if (data.IsDataTable()) return "datatable";
            if (data.IsBundle()) return "bundle";
            if (data.IsSliceBundle()) return "slcbundle";
            if (data.IsPointerBundle()) return "ptrbundle";
            if (data.IsByteBundle()) return "bytebundle";
            if (data.IsDDSBundle()) return "ddsbundle";
            return "bin";
        }

        private static bool TryExtractDataTable(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new DataTable(data);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("DTBL", "Failed unpacking DataTable", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractSCEN(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new SCEN(data);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("SCEN", "Failed unpacking SCEN", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractBundle(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new Bundle(data);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("BUN", "Failed unpacking Bundle", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractSliceBundle(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new SliceBundle(data);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("SBUN", "Failed unpacking Bundle", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractPointerBundle(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new PointerBundle(data);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("PBUN", "Failed unpacking Pointer Bundle", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractByteBundle(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new ByteBundle(data);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("BBUN", "Failed unpacking Pointer Bundle", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractMDLK(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new MDLK(data);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, false, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("MDLK", "Failed unpacking MDLK", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractXL(string pathBase, Span<byte> data)
        {
            try
            {
                var blobs = new XL19(data);
                if (blobs.Entries.Count == 0) return true;

                var ft = Path.ChangeExtension(pathBase, ".json");
                var basedir = Path.GetDirectoryName(ft);

                if (!Directory.Exists(basedir))
                {
                    if (File.Exists(basedir))
                    {
                        Logger.Warn("XL", $@"Trying to make a directory named {basedir} but it is a file?");
                        return false;
                    }

                    Directory.CreateDirectory(basedir);
                }

                File.WriteAllText(ft, JsonConvert.SerializeObject(new
                {
                    blobs.Types,
                    blobs.Entries
                }, Formatting.Indented, new StringEnumConverter()));
                Logger.Info("XL", ft);
            }
            catch (Exception e)
            {
                Logger.Error("XL", "Failed unpacking XL", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractG1M(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new G1Model(data, true, false);
                if (blobs.SectionRoot.Sections.Count == 0) return true;

                for (var index = 0; index < blobs.SectionRoot.Sections.Count; index++)
                {
                    var sectionData = blobs.SectionRoot.Sections[index];
                    var magic = MemoryMarshal.Read<DataType>(sectionData.Span);
                    TryExtractBlob($@"{pathBase}\{index:X4}.{string.Join(string.Empty, magic.ToFourCC(true).Reverse())}", sectionData.Span, false, flags, false);
                }
            }
            catch (Exception e)
            {
                Logger.Error("G1M", "Failed unpacking G1M", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractKTSR(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new SoundResource(data, flags.Platform);
                if (blobs.Entries.Count == 0) return true;

                foreach (var datablob in blobs.Entries)
                {
                    switch (datablob)
                    {
                        case NamedSounds adpcm:
                        {
                            foreach (var adcpmSection in adpcm.Sections)
                            {
                                switch (adcpmSection)
                                {
                                    case GCADPCMSound gcadpcm:
                                    {
                                        var streams = !flags.RawKTSR && !flags.SkipADPCM ? gcadpcm.ReconstructWave(!flags.SkipADPCM) : gcadpcm.ReconstructAsIndividual();
                                        TryExtractBlobs($@"{pathBase}\{datablob.Base.Id:X8}_{gcadpcm.Base.Id:X8}", streams, false, null, streams.Count == 1, true, false, !flags.RawKTSR && !flags.SkipADPCM ? "wav" : "ktgcadpcm", flags);
                                        break;
                                    }
                                    case MSADPCMSound msadpcm:
                                    {
                                        var streams = flags.RawKTSR ? msadpcm.ReconstructAsIndividual() : msadpcm.ReconstructWave(!flags.SkipADPCM);
                                        TryExtractBlobs($@"{pathBase}\{datablob.Base.Id:X8}_{msadpcm.Base.Id:X8}", streams, false, null, streams.Count == 1, true, false, flags.RawKTSR ? "ktmsadpcm" : "wav", flags);
                                        break;
                                    }
                                    default:
                                        TryExtractBlob($@"{pathBase}\{datablob.Base.Id:X8}_{adcpmSection.Base.Id:X8}.{GetExtension(adcpmSection.FullBuffer.Span)}", adcpmSection.FullBuffer.Span, false, flags, false);
                                        break;
                                }
                            }

                            break;
                        }
                        case GCADPCMSound gcadpcm:
                        {
                            var streams = !flags.RawKTSR && !flags.SkipADPCM ? gcadpcm.ReconstructWave(!flags.SkipADPCM) : gcadpcm.ReconstructAsIndividual();
                            TryExtractBlobs($@"{pathBase}\{datablob.Base.Id:X8}_{gcadpcm.Base.Id:X8}", streams, false, null, streams.Count == 1, true, false, !flags.RawKTSR && !flags.SkipADPCM ? "wav" : "ktgcadpcm", flags);
                            break;
                        }
                        case MSADPCMSound msadpcm:
                        {
                            var streams = flags.RawKTSR ? msadpcm.ReconstructAsIndividual() : msadpcm.ReconstructWave(!flags.SkipADPCM);
                            TryExtractBlobs($@"{pathBase}\{datablob.Base.Id:X8}_{msadpcm.Base.Id:X8}", streams, false, null, streams.Count == 1, true, false, flags.RawKTSR ? "ktmsadpcm" : "wav", flags);
                            break;
                        }
                        default:
                        {
                            var buffer = datablob switch
                            {
                                OGGSound sample => sample.Data,
                                _ => datablob.FullBuffer
                            };
                            TryExtractBlob($@"{pathBase}\{datablob.Base.Id:X8}.{GetExtension(buffer.Span)}", buffer.Span, false, flags, false);
                            break;
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error("KTSR", "Failed unpacking KTSR", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }
        }

        private static bool TryExtractKTSC(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new SoundContainer(data);
                for (var index = 0; index < blobs.KTSR.Count; index++)
                {
                    var ktsr = blobs.KTSR[index];
                    TryExtractBlob($@"{pathBase}\{blobs.Identifiers[index]:X8}.{GetExtension(ktsr.Span)}", ktsr.Span, false, flags, false);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error("KTSR", "Failed unpacking KTSR", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }
        }

        private static bool TryExtractG1L(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new G1L(data);
                var buffer = blobs.Buffer;
                TryExtractBlob(Path.ChangeExtension(pathBase, "kvs"), buffer.Span, false, flags, false);
            }
            catch (Exception e)
            {
                Logger.Error("G1L", "Failed unpacking G1L", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractKOVS(string pathBase, Span<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new KOVSSound(data);
                var buffer = blobs.Stream;
                TryExtractBlob(Path.ChangeExtension(pathBase, "ogg"), buffer.Span, false, flags, false);
            }
            catch (Exception e)
            {
                Logger.Error("KOVS", "Failed unpacking KOVS", e);
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }
    }
}
