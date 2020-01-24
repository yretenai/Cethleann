using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Audio;
using Cethleann.DataTables;
using Cethleann.G1;
using Cethleann.Koei;
using Cethleann.Text;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Unbundler
{
    [PublicAPI]
    public static class UnbundlerLogic
    {
        public static void TryExtractBlobs(string pathBase, List<Memory<byte>> blobs, bool allTypes, List<string> names, bool singleFile, string extension, UnbundlerFlags flags)
        {
            for (var index = 0; index < blobs.Count; index++)
            {
                var datablob = blobs[index];
                var name = $"{index:X4}";
                var foundName = names?.ElementAtOrDefault(index);
                extension ??= GetExtension(datablob.Span);
                if (foundName != null)
                {
                    name = foundName;
                    if (File.Exists($@"{pathBase}\{name}.{extension}"))
                    {
                        var oname = name + "_";
                        var i = 1;
                        while (File.Exists($@"{pathBase}\{name}.{extension}")) name = oname + $"{i++:X}";
                    }
                }

                TryExtractBlob($@"{pathBase}\{name}.{extension}", datablob, allTypes, singleFile && blobs.Count == 1, flags);
            }
        }

        public static int TryExtractBlob(string blobBase, Memory<byte> datablob, bool allTypes, bool singleFile, UnbundlerFlags flags)
        {
            if (datablob.Length == 0 && !flags.WriteZero)
            {
                Logger.Info("KTGL", $"{blobBase} is zero!");
                return 0;
            }

            var dataType = datablob.Span.GetDataType();

            if (allTypes || flags.Recursive)
            {
                if (!datablob.Span.IsKnown() && datablob.Span.IsDataTable())
                    if (TryExtractDataTable(blobBase, datablob, flags))
                        return 1;
                if (datablob.Span.IsBundle())
                    if (TryExtractBundle(blobBase, datablob, flags))
                        return 1;
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (dataType)
                {
                    case DataType.SCEN when TryExtractSCEN(blobBase, datablob, flags):
                    case DataType.MDLK when TryExtractMDLK(blobBase, datablob, flags):
                    case DataType.KTSR when TryExtractKTSR(blobBase, datablob, flags):
                    case DataType.KTSC when TryExtractKTSC(blobBase, datablob, flags):
                    case DataType.Model when !flags.Recursive && TryExtractG1M(blobBase, datablob, flags):
                    case DataType.TextLocalization19 when TryExtractLX(blobBase, datablob):
                    case DataType.GAPK when TryExtractGAPK(blobBase, datablob, flags, false):
                    case DataType.GEPK when TryExtractGAPK(blobBase, datablob, flags, true):
                    case DataType.GMPK when TryExtractGMPK(blobBase, datablob, flags):
                    case DataType.LosslessAudio when TryExtractG1L(blobBase, datablob, flags):
                    case DataType.KOVS when TryExtractKOVS(blobBase, datablob, flags):
                    case DataType.RTRPK when TryExtractRTRPK(blobBase, datablob, flags):
                    case DataType.WHD when TryExtractWHD(blobBase, datablob, flags):
                        return 1;
                }
            }

            if (dataType == DataType.Compressed || dataType == DataType.CompressedChonky)
                try
                {
                    var decompressed = Compression.Decompress(datablob.Span);
                    var pathBase = blobBase;
                    if (pathBase.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase)) pathBase = pathBase.Substring(0, pathBase.Length - 3);

                    var result = TryExtractBlob(pathBase, new Memory<byte>(decompressed.ToArray()), allTypes, singleFile, flags);

                    if (result > 0) return result;
                }
                catch (Exception e)
                {
                    Logger.Error("KTGL", $"Failed decompressing blob, {e}");
                }

            var basedir = Path.GetDirectoryName(blobBase);
            if (singleFile)
            {
                blobBase = $@"{basedir}{Path.GetExtension(blobBase)}";
                basedir = Path.GetDirectoryName(blobBase);
            }

            if (!Directory.Exists(basedir))
            {
                if (File.Exists(basedir))
                {
                    Logger.Warn("KTGL", $@"Trying to make a directory named {basedir} but it is a file?");
                    return 0;
                }

                Directory.CreateDirectory(basedir);
            }

            if (File.Exists(blobBase))
            {
                if (flags.Overwrite)
                {
                    File.Delete(blobBase);
                }
                else
                {
                    Logger.Warn("KTGL", $@"{blobBase} already exists!");
                    return 0;
                }
            }

            Logger.Info("KTGL", blobBase);
            File.WriteAllBytes(blobBase, datablob.ToArray());

            return 2;
        }

        private static bool TryExtractGAPK(string pathBase, Memory<byte> data, UnbundlerFlags flags, bool prependNames)
        {
            try
            {
                var blobs = new GAPK(data.Span);
                if (blobs.Blobs.Count == 0) return true;

                var names = blobs.NameMap.Names;
                if (prependNames)
                {
                    names.Insert(0, blobs.NameMap.Name ?? Path.GetFileName(pathBase));
                    names.Insert(0, blobs.NameMap.Name ?? Path.GetFileName(pathBase));
                }

                TryExtractBlobs(pathBase, blobs.Blobs, false, names, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("GAPK", $"Failed unpacking GAPK, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractRTRPK(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new RTRPK(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("RTRPK", $"Failed unpacking RTRPK, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractWHD(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new WaveHeaderData(data.Span, flags.WBHAlternateNames);
                if (blobs.WBH.Soundbank.Entries.Count == 0) return true;

                var names = blobs.WBH.Soundbank.Names;
                for (var index = 0; index < blobs.WBH.Soundbank.Entries.Count; index++)
                {
                    var stream = blobs.WBH.Soundbank.Entries[index];
                    var wav = blobs.WBD.ReconstructWave(stream);
                    TryExtractBlob($@"{pathBase}\{(names?.ElementAtOrDefault(index) ?? index.ToString("X8"))}.wav", wav, false, false, flags);
                }
            }
            catch (Exception e)
            {
                Logger.Error("RTRPK", $"Failed unpacking RTRPK, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractGMPK(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new GMPK(data.Span);
                if (blobs.Blobs.Count == 0) return true;

                var nameIndex = 0;
                var names = new List<string>();
                foreach (var blob in blobs.Blobs)
                {
                    // there probably is a smart way of doing this
                    // but this works 9 out of 10 times.
                    if (blob.Span.GetDataType() == DataType.TextureGroup) nameIndex--;
                    if (blob.Span.GetDataType() == DataType.SWGQ) nameIndex--;
                    if (nameIndex >= blobs.NameMap.Names.Count) break;
                    names.Add(blobs.NameMap.Names[nameIndex]);
                    nameIndex++;
                }

                TryExtractBlobs(pathBase, blobs.Blobs, false, names, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("GMPK", $"Failed unpacking GMPK, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        public static string GetExtension(Span<byte> data)
        {
            var dt = data.GetDataType();
            if (!data.IsKnown() && data.IsDataTable()) return "datatable";
            if (dt == DataType.SCEN) return "scene";
            if (data.IsBundle()) return "bundle";
            if (dt == DataType.MDLK) return "mdlk";
            return dt.GetExtension();
        }

        private static bool TryExtractDataTable(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new DataTable(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("DTBL", $"Failed unpacking DataTable, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractSCEN(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new SCEN(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("SCEN", $"Failed unpacking SCEN, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractBundle(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new Bundle(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("BUN", $"Failed unpacking Bundle, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractMDLK(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new MDLK(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, null, false, null, flags);
            }
            catch (Exception e)
            {
                Logger.Error("MDLK", $"Failed unpacking MDLK, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractLX(string pathBase, Memory<byte> data)
        {
            try
            {
                var blobs = new TextLocalization(data.Span);
                if (blobs.Entries.Count == 0) return true;

                var ft = Path.ChangeExtension(pathBase, ".txt");
                var basedir = Path.GetDirectoryName(ft);

                if (!Directory.Exists(basedir))
                {
                    if (File.Exists(basedir))
                    {
                        Logger.Warn("LX", $@"Trying to make a directory named {basedir} but it is a file?");
                        return false;
                    }

                    Directory.CreateDirectory(basedir);
                }

                var lines = string.Join(Environment.NewLine, blobs.Entries.SelectMany((x, i) => x.Select((y, j) => $"{i},{j} = " + y.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r"))));
                File.WriteAllText(ft, lines);
                Logger.Info("LX", ft);
            }
            catch (Exception e)
            {
                Logger.Error("LX", $"Failed unpacking LX, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractG1M(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new G1Model(data.Span, true, false);
                if (blobs.SectionRoot.Sections.Count == 0) return true;

                for (var index = 0; index < blobs.SectionRoot.Sections.Count; index++)
                {
                    var sectionData = blobs.SectionRoot.Sections[index];
                    var magic = MemoryMarshal.Read<DataType>(sectionData.Span);
                    TryExtractBlob($@"{pathBase}\{index:X4}.{string.Join("", magic.ToFourCC(true).Reverse())}", sectionData, false, false, flags);
                }
            }
            catch (Exception e)
            {
                Logger.Error("G1M", $"Failed unpacking G1M, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractKTSR(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new SoundResource(data.Span, flags.Platform);
                if (blobs.Entries.Count == 0) return true;

                foreach (var datablob in blobs.Entries)
                {
                    if (datablob is NamedSounds adpcm)
                    {
                        foreach (var adcpmSection in adpcm.Sections)
                        {
                            if (adcpmSection is GCADPCMSound gcadpcm)
                            {
                                var streams = gcadpcm.RebuildAsIndividual();
                                TryExtractBlobs($@"{pathBase}\{datablob.Base.Id:X8}_{gcadpcm.Base.Id:X8}", streams, false, null, streams.Count == 1, "ktgcadpcm", flags);
                            }
                            else if (adcpmSection is MSADPCMSound msadpcm)
                            {
                                var streams = msadpcm.RebuildAsIndividual();
                                TryExtractBlobs($@"{pathBase}\{datablob.Base.Id:X8}_{msadpcm.Base.Id:X8}", streams, false, null, streams.Count == 1, "ktmsadpcm", flags);
                            }
                            else
                            {
                                TryExtractBlob($@"{pathBase}\{datablob.Base.Id:X8}_{adcpmSection.Base.Id:X8}.{GetExtension(adcpmSection.FullBuffer.Span)}", adcpmSection.FullBuffer, false, false, flags);
                            }
                        }
                    }
                    else if (datablob is GCADPCMSound gcadpcm)
                    {
                        var streams = gcadpcm.RebuildAsIndividual();
                        TryExtractBlobs($@"{pathBase}\{datablob.Base.Id:X8}", streams, false, null, streams.Count == 1, "ktgcadpcm", flags);
                    }
                    else if (datablob is MSADPCMSound msadpcm)
                    {
                        var streams = msadpcm.RebuildAsIndividual();
                        TryExtractBlobs($@"{pathBase}\{datablob.Base.Id:X8}", streams, false, null, streams.Count == 1, "ktmsadpcm", flags);
                    }
                    else
                    {
                        var buffer = datablob switch
                        {
                            OGGSound sample => sample.Data,
                            _ => datablob.FullBuffer
                        };
                        TryExtractBlob($@"{pathBase}\{datablob.Base.Id:X8}.{GetExtension(buffer.Span)}", buffer, false, false, flags);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error("KTSR", $"Failed unpacking KTSR, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }
        }

        private static bool TryExtractKTSC(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new SoundContainer(data.Span);
                for (var index = 0; index < blobs.KTSR.Count; index++)
                {
                    var ktsr = blobs.KTSR[index];
                    TryExtractBlob($@"{pathBase}\{blobs.Identifiers[index]:X8}.{GetExtension(ktsr.Span)}", ktsr, false, false, flags);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error("KTSR", $"Failed unpacking KTSR, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }
        }

        private static bool TryExtractG1L(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new G1Lossless(data.Span);
                var buffer = blobs.Audio;
                TryExtractBlob($@"{pathBase}\{0:X4}.{GetExtension(buffer.Span)}", buffer, false, true, flags);
            }
            catch (Exception e)
            {
                Logger.Error("G1L", $"Failed unpacking G1L, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractKOVS(string pathBase, Memory<byte> data, UnbundlerFlags flags)
        {
            try
            {
                var blobs = new KOVSSound(data.Span);
                var buffer = blobs.Stream;
                TryExtractBlob($@"{pathBase}\{0:X4}.{GetExtension(buffer.Span)}", buffer, false, true, flags);
            }
            catch (Exception e)
            {
                Logger.Error("KOVS", $"Failed unpacking KOVS, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }
    }
}
