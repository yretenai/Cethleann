using Cethleann.Structure;
using DragonLib;
using DragonLib.CLI;
using DragonLib.IO;
using System;
using System.IO;

namespace Cethleann.Identify
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Cethleann");
            var flags = CommandLineFlags.ParseFlags<IdentifyFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;

            foreach (var file in flags.Paths)
            {
                if (file == null || !File.Exists(file)) continue;
                Console.Write($"{file} ");
                var datablob = new Span<byte>(File.ReadAllBytes(file));
                var type = flags.ForceType == DataType.None ? datablob.GetDataType() : flags.ForceType;
                if (!datablob.IsKnown())
                {
                    if (datablob.IsDataTable())
                        if (PrintDataTable(datablob, flags))
                            continue;
                    if (datablob.IsBundle())
                        if (PrintBundle(datablob, flags))
                            continue;
                    if (datablob.IsSliceBundle())
                        if (PrintSliceBundle(datablob, flags))
                            continue;
                    if (datablob.IsPointerBundle())
                        if (PrintPointerBundle(datablob, flags))
                            continue;
                    if (datablob.IsByteBundle())
                        if (PrintByteBundle(datablob, flags))
                            continue;
                }

                switch (type)
                {
                    case DataType.SCEN when PrintSCEN(datablob, flags):
                    case DataType.MDLK when PrintMDLK(datablob, flags):
                    case DataType.KTSR when PrintKTSR(datablob, flags):
                    case DataType.KTSC when PrintKTSC(datablob, flags):
                    case DataType.Model when PrintG1M(datablob, flags):
                    case DataType.XL when PrintXL(datablob):
                    case DataType.GAPK when PrintGAPK(datablob, flags, false):
                    case DataType.GEPK when PrintGAPK(datablob, flags, true):
                    case DataType.GMPK when PrintGMPK(datablob, flags):
                    case DataType.Large when PrintG1L(datablob, flags):
                    case DataType.KOVS when PrintKOVS(datablob, flags):
                    case DataType.ElixirArchive when PrintElixir(datablob, flags):
                    case DataType.RTRPK when PrintRESPACK(datablob, flags):
                    case DataType.EFFRESPK when PrintRESPACK(datablob, flags):
                    case DataType.TDPACK when PrintRESPACK(datablob, flags):
                    case DataType.COLRESPK when PrintRESPACK(datablob, flags):
                    case DataType.MDLRESPK when PrintRESPACK(datablob, flags):
                    case DataType.MDLTEXPK when PrintRESPACK(datablob, flags):
                    case DataType.EXARG when PrintRESPACK(datablob, flags):
                    case DataType.TRRRESPK when PrintRESPACK(datablob, flags):
                    case DataType.G1E_PACK when PrintRESPACK(datablob, flags):
                    case DataType.G1M_PACK when PrintRESPACK(datablob, flags):
                    case DataType.G1H_PACK when PrintRESPACK(datablob, flags):
                    case DataType.G2A_PACK when PrintRESPACK(datablob, flags):
                    case DataType.HEADPACK when PrintRESPACK(datablob, flags):
                    case DataType.G1COPACK when PrintRESPACK(datablob, flags):
                    case DataType.WHD when PrintWHD(datablob, flags):
                        continue;
                    case DataType.None:
                        break;
                    case DataType.LINKDATA:
                        break;
                    case DataType.StructTable:
                        break;
                    case DataType.ECB:
                        break;
                    case DataType.SARC:
                        break;
                    case DataType.SPKG:
                        break;
                    case DataType.KTSS:
                        break;
                    case DataType.OGG:
                        break;
                    case DataType.SPK3:
                        break;
                    case DataType.SwingDefinition:
                        break;
                    case DataType.River:
                        break;
                    case DataType.RIGB:
                        break;
                    case DataType.RIGBL:
                        break;
                    case DataType.ERTR:
                        break;
                    case DataType.DATD:
                        break;
                    case DataType.LCD0:
                        break;
                    case DataType.WEBM:
                        break;
                    case DataType.ScreenLayout:
                        break;
                    case DataType.ScreenLayoutTexture:
                        break;
                    case DataType.VideoSource:
                        break;
                    case DataType.Font2:
                        break;
                    case DataType.Morph:
                        break;
                    case DataType.Collision:
                        break;
                    case DataType.XL20:
                        break;
                    case DataType.TextureGroup:
                        break;
                    case DataType.AnimationV2:
                        break;
                    case DataType.Animation:
                        break;
                    case DataType.Shader:
                        break;
                    case DataType.EffectManager:
                        break;
                    case DataType.Effect:
                        break;
                    case DataType.SEBin:
                        break;
                    case DataType.PostFX:
                        break;
                    case DataType.G1MS:
                        break;
                    case DataType.G1MF:
                        break;
                    case DataType.G1MG:
                        break;
                    case DataType.G1MM:
                        break;
                    case DataType.EXTR:
                        break;
                    case DataType.COLL:
                        break;
                    case DataType.NUNO:
                        break;
                    case DataType.NUNV:
                        break;
                    case DataType.NUNS:
                        break;
                    case DataType.SOFT:
                        break;
                    case DataType.HAIR:
                        break;
                    case DataType.LMPK:
                        break;
                    case DataType.BPK:
                        break;
                    case DataType.RIFF:
                        break;
                    case DataType.HDDB:
                        break;
                    case DataType.WBH:
                        break;
                    case DataType.WBD:
                        break;
                    case DataType.RDB:
                        break;
                    case DataType.RDBIndex:
                        break;
                    case DataType.PortableRDB:
                        break;
                    case DataType.NDB:
                        break;
                    case DataType.NDBIndex:
                        break;
                    case DataType.OBJDB:
                        break;
                    case DataType.OBJDBIndex:
                        break;
                    case DataType.OBJDBRecord:
                        break;
                    case DataType.Compressed:
                        break;
                    case DataType.CompressedChonky:
                        break;
                    case DataType.PackageInfo:
                        break;
                    case DataType.WMV:
                        break;
                    case DataType.DDS:
                        break;
                    case DataType.CharacterData:
                        break;
                    case DataType.CONT:
                        break;
                    case DataType.TMC:
                        break;
                    case DataType.Clip:
                        break;
                    case DataType.Body:
                        break;
                    case DataType.PBSMaterial:
                        break;
                    case DataType.OldTDPack:
                        break;
                    default:
                        Console.WriteLine($"unknown {type.ToFourCC(true)} {(int) type:X8}  {((ulong) datablob.Length).GetHumanReadableBytes()}");
                        continue;
                }
            }
        }

        private static bool PrintSCEN(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintMDLK(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintKTSR(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintKTSC(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintG1M(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintXL(in Span<byte> datablob) => throw new NotImplementedException();

        private static bool PrintGAPK(in Span<byte> datablob, IdentifyFlags? flags, bool p2) => throw new NotImplementedException();

        private static bool PrintGMPK(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintG1L(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintKOVS(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintElixir(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintRESPACK(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintWHD(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintByteBundle(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintPointerBundle(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintSliceBundle(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintBundle(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();

        private static bool PrintDataTable(in Span<byte> datablob, IdentifyFlags? flags) => throw new NotImplementedException();
    }
}
