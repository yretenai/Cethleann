using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann;
using Cethleann.Audio;
using Cethleann.DataTables;
using Cethleann.G1;
using Cethleann.ManagedFS;
using Cethleann.Text;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Koei.DataExporter
{
    [PublicAPI]
    public static class Program
    {
        public static bool Recursive { get; set; }

        private static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Logger.Info("KTGL", "Usage: Koei.DataExporter.exe RomFS output <recursive yes/no> [PatchRomFS [DLCRomFS...]]");
                Logger.Info("KTGL", "Example: Koei.DataExporter.exe NCA/BaseGame/RomFS FETH yes NCA/Update/RomFS/patch3");
                Logger.Warn("KTGL", "Setting recursive to YES will output a LOT of files.");
                return;
            }

            var romfs = args.First();
            var output = args.ElementAt(1);
            Recursive = args.ElementAt(2).ToLower()[0] == 'y';
            using var cethleann = new Flayn(romfs, GameId.FireEmblemThreeHouses);

            if (args.Length > 4 && Directory.Exists(args.ElementAt(3))) cethleann.AddPatchFS(args.ElementAt(3));

            foreach (var dlcromfs in args.Skip(4)) cethleann.AddDataFS(dlcromfs);
            cethleann.TestDLCSanity();
            cethleann.LoadFileList();
            ExtractAll(output, cethleann);
        }

        private static void ExtractAll(string romfs, Flayn cethleann)
        {
            if (!Directory.Exists($@"{romfs}\romfs")) Directory.CreateDirectory($@"{romfs}\romfs");
            for (var index = 0; index < cethleann.EntryCount; index++)
            {
                var data = cethleann.ReadEntry(index);
                var dt = data.Span.GetDataType();
                var ext = GetExtension(data.Span);
                var pathBase = $@"{romfs}\romfs\{cethleann.GetFilename(index, ext, dt)}";
                TryExtractBlob(pathBase, data, false, false, false);
            }
        }

        public static void TryExtractBlobs(string pathBase, List<Memory<byte>> blobs, bool allTypes, bool writeZero, List<string> names, bool singleFile)
        {
            for (var index = 0; index < blobs.Count; index++)
            {
                var datablob = blobs[index];
                var name = $"{index:X4}";
                var foundName = names?.ElementAtOrDefault(index);
                if (foundName != null)
                {
                    name = foundName;
                    if (File.Exists($@"{pathBase}\{name}.{GetExtension(datablob.Span)}"))
                    {
                        var oname = name + "_";
                        var i = 1;
                        while (File.Exists($@"{pathBase}\{name}.{GetExtension(datablob.Span)}")) name = oname + $"{i++:X}";
                    }
                }

                TryExtractBlob($@"{pathBase}\{name}.{GetExtension(datablob.Span)}", datablob, allTypes, writeZero, singleFile && blobs.Count == 1);
            }
        }

        public static int TryExtractBlob(string blobBase, Memory<byte> datablob, bool allTypes, bool writeZero, bool singleFile)
        {
            if (datablob.Length == 0 && !writeZero)
            {
                Logger.Info("KTGL", $"{blobBase} is zero!");
                return 0;
            }

            if (allTypes || Recursive)
            {
                if (!datablob.Span.IsKnown() && datablob.Span.IsDataTable())
                    if (TryExtractDataTable(blobBase, datablob, writeZero))
                        return 1;
                if (datablob.Span.IsBundle())
                    if (TryExtractBundle(blobBase, datablob, writeZero))
                        return 1;
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (datablob.Span.GetDataType())
                {
                    case DataType.SCEN when TryExtractSCEN(blobBase, datablob, writeZero):
                    case DataType.KLDM when TryExtractKLDM(blobBase, datablob, writeZero):
                    case DataType.KTSR when TryExtractKTSR(blobBase, datablob, writeZero):
                    case DataType.Model when TryExtractG1M(blobBase, datablob, writeZero):
                    case DataType.TextLocalization19 when TryExtractLX(blobBase, datablob):
                    case DataType.GAPK when TryExtractGAPK(blobBase, datablob, writeZero, false):
                    case DataType.GEPK when TryExtractGAPK(blobBase, datablob, writeZero, true):
                    case DataType.GMPK when TryExtractGMPK(blobBase, datablob, writeZero):
                    case DataType.LosslessAudio when TryExtractG1L(blobBase, datablob, writeZero):
                    case DataType.KOVS when TryExtractKOVS(blobBase, datablob, writeZero):
                        return 1;
                }
            }


            var basedir = Path.GetDirectoryName(blobBase);
            if (singleFile)
            {
                blobBase = $@"{basedir}{Path.GetExtension(blobBase)}";
            }
            else if (!Directory.Exists(basedir))
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
                Logger.Warn("KTGL", $@"{blobBase} already exists!");
                return 0;
            }

            Logger.Info("KTGL", $@"{blobBase}");
            File.WriteAllBytes(blobBase, datablob.ToArray());

            return 2;
        }

        private static bool TryExtractGAPK(string pathBase, Memory<byte> data, bool writeZero, bool prependNames)
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

                TryExtractBlobs(pathBase, blobs.Blobs, false, writeZero, names, false);
            }
            catch (Exception e)
            {
                Logger.Error("GAPK", $"Failed unpacking GAPK, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractGMPK(string pathBase, Memory<byte> data, bool writeZero)
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

                TryExtractBlobs(pathBase, blobs.Blobs, false, writeZero, names, false);
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
        private static string GetExtension(Span<byte> data)
        {
            var dt = data.GetDataType();
            if (!data.IsKnown() && data.IsDataTable()) return "datatable";
            if (dt == DataType.SCEN) return "scene";
            if (data.IsBundle()) return "bundle";
            if (dt == DataType.KLDM) return "kldm";
            return dt.GetExtension();
        }

        private static bool TryExtractDataTable(string pathBase, Memory<byte> data, bool writeZero)
        {
            try
            {
                var blobs = new DataTable(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, writeZero, null, false);
            }
            catch (Exception e)
            {
                Logger.Error("DTBL", $"Failed unpacking DataTable, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractSCEN(string pathBase, Memory<byte> data, bool writeZero)
        {
            try
            {
                var blobs = new SCEN(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, writeZero, null, false);
            }
            catch (Exception e)
            {
                Logger.Error("SCEN", $"Failed unpacking SCEN, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractBundle(string pathBase, Memory<byte> data, bool writeZero)
        {
            try
            {
                var blobs = new Bundle(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, writeZero, null, false);
            }
            catch (Exception e)
            {
                Logger.Error("BUN", $"Failed unpacking Bundle, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractKLDM(string pathBase, Memory<byte> data, bool writeZero)
        {
            try
            {
                var blobs = new KLDM(data.Span);
                if (blobs.Entries.Count == 0) return true;

                TryExtractBlobs(pathBase, blobs.Entries, false, writeZero, null, false);
            }
            catch (Exception e)
            {
                Logger.Error("KLDM", $"Failed unpacking KLDM, {e}");
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
                var lines = string.Join(Environment.NewLine, blobs.Entries.SelectMany((x, i) => x.Select((y, j) => $"{i},{j} = " + y.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r"))));
                File.WriteAllText(ft, lines);
            }
            catch (Exception e)
            {
                Logger.Error("KLDM", $"Failed unpacking KLDM, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractG1M(string pathBase, Memory<byte> data, bool writeZero)
        {
            try
            {
                var blobs = new G1Model(data.Span, true, false);
                if (blobs.SectionRoot.Sections.Count == 0) return true;

                for (var index = 0; index < blobs.SectionRoot.Sections.Count; index++)
                {
                    var sectionData = blobs.SectionRoot.Sections[index];
                    var magic = MemoryMarshal.Read<DataType>(sectionData.Span);
                    TryExtractBlob($@"{pathBase}\{index:X4}.{string.Join("", magic.ToFourCC(true).Reverse())}", sectionData, false, writeZero, false);
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

        private static bool TryExtractKTSR(string pathBase, Memory<byte> data, bool writeZero)
        {
            try
            {
                var blobs = new SoundResource(data.Span);
                if (blobs.Entries.Count == 0) return true;

                foreach (var datablob in blobs.Entries)
                {
                    var buffer = datablob switch
                    {
                        SoundResourceSample sample => sample.Data.FullBuffer,
                        SoundUnknown unknown => unknown.Data,
                        _ => Memory<byte>.Empty
                    };
                    TryExtractBlob($@"{pathBase}\{datablob.Base.Id:X8}.{GetExtension(buffer.Span)}", buffer, false, writeZero, false);
                }
            }
            catch (Exception e)
            {
                Logger.Error("KTSR", $"Failed unpacking KTSR, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractG1L(string pathBase, Memory<byte> data, bool writeZero)
        {
            try
            {
                var blobs = new G1Lossless(data.Span);
                var buffer = blobs.Audio;
                TryExtractBlob($@"{pathBase}\{0:X4}.{GetExtension(buffer.Span)}", buffer, false, writeZero, true);
            }
            catch (Exception e)
            {
                Logger.Error("G1L", $"Failed unpacking G1L, {e}");
                if (Directory.Exists(pathBase)) Directory.Delete(pathBase, true);

                return false;
            }

            return true;
        }

        private static bool TryExtractKOVS(string pathBase, Memory<byte> data, bool writeZero)
        {
            try
            {
                var blobs = new KOVS(data.Span);
                var buffer = blobs.Stream;
                TryExtractBlob($@"{pathBase}\{0:X4}.{GetExtension(buffer.Span)}", buffer, false, writeZero, true);
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
