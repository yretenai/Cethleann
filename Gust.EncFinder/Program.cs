using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DragonLib;
using DragonLib.IO;

namespace Gust.EncFinder
{
    static class Program
    {
        private const string SEED_CONSTANT_SIGNATURE = "41 C7 ?? ?? C9 73 9A 3B";
        private const string SEED_CONSTANT_SIGNATURE_2 = "41 C7 ?? C9 73 9A 3B";
        private const string FUNCTION_START_SIGNATURE = "CC 48 89 5C 24 ?? 48 89 54 24 10";
        private const string TABLE_START_SIGNATURE = "C7 45";
        private const string MOV_DWORD_LITERAL_SIGNATURE = "41 B9";
        private const string MOV_DWORD_PTR_SIGNATURE = "41 C7";
        private const string MOV_DWORD_PTR_SIGNATURE_2 = "04";
        private const string FENCE_SIGNATURE = "69 ?? ?? ?? 00 00 2B";


        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Logger.Error(null, "Usage: Gust.EncFinder.exe path/to/file.exe");
                return 1;
            }

            foreach (var pefile in args)
            {
                Console.WriteLine(Path.GetFileName(pefile));
                Span<byte> exe;
                using (var stream = File.OpenRead(pefile))
                {
                    exe = new Span<byte>(new byte[stream.Length]);
                    stream.Read(exe);
                }

                var constantPtr = exe.FindPointerFromSignature(SEED_CONSTANT_SIGNATURE);
                if (constantPtr == -1)
                {
                    constantPtr = exe.FindPointerFromSignature(SEED_CONSTANT_SIGNATURE_2);
                    if (constantPtr == -1)
                    {
                        Logger.Error(null, "Can't find the seed constant MOV instruction, is the executable obfuscated or not PE32?");
                        return 2;
                    }
                }

                LogNumber("Seed Constant MOV Instruction", constantPtr);

                var functionStartPtr = exe.Slice(0, constantPtr).FindPointerFromSignatureReverse(FUNCTION_START_SIGNATURE);
                if (functionStartPtr == -1)
                {
                    Logger.Error(null, "Can't find the start of the function, is the executable obfuscated or not PE32?");
                    return 3;
                }

                LogNumber("EncDec::fileReadWriteEncodeDecodeData Function", functionStartPtr);

                var table = new int[3];
                var lengths = new int[3];
                var main = new int[3];

                var counter = 0;
                var tablePtr = 0;
                while (true)
                {
                    var localTablePtr = exe.Slice(functionStartPtr + tablePtr, constantPtr - tablePtr - functionStartPtr).FindPointerFromSignature(TABLE_START_SIGNATURE);
                    if (localTablePtr == -1)
                    {
                        break;
                    }

                    tablePtr += localTablePtr + 2;
                    LogNumber($"Initialization MOV Instruction {counter}", functionStartPtr + tablePtr - 2);

                    var num = MemoryMarshal.Read<int>(exe.Slice(functionStartPtr + tablePtr + 1));
                    if (num == 0 && counter == 0)
                    {
                        Logger.Warn(null, "Skipping first table entry because it is zero.");
                        continue; // weird edge case for Nights of Azure 2
                    }

                    if (counter >= 3)
                    {
                        table[counter - 3] = num;
                    }
                    else
                    {
                        lengths[counter] = num;
                    }

                    counter++;

                    if (counter >= 6)
                    {
                        break;
                    }
                }

                if (table.Contains(0) || lengths.Contains(0))
                {
                    Logger.Error(null, "Table is incomplete?");
                    return 3;
                }

                var main1Ptr = exe.Slice(constantPtr).FindPointerFromSignature(MOV_DWORD_LITERAL_SIGNATURE);
                if (main1Ptr == -1)
                {
                    Logger.Error(null, "Can't find main[0]");
                    return 4;
                }

                LogNumber("Main table MOV instruction 1", functionStartPtr);
                main[0] = MemoryMarshal.Read<int>(exe.Slice(main1Ptr + constantPtr + 2));

                var main2Ptr = exe.Slice(main1Ptr + constantPtr).FindPointerFromSignature(MOV_DWORD_PTR_SIGNATURE);
                var main2Ptr2 = exe.Slice(main2Ptr + main1Ptr + constantPtr).FindPointerFromSignature(MOV_DWORD_PTR_SIGNATURE_2);
                if (main2Ptr == -1)
                {
                    Logger.Error(null, "Can't find main[2]");
                    return 4;
                }

                LogNumber("Main table MOV instruction 2", main2Ptr + main1Ptr + constantPtr);
                main[1] = MemoryMarshal.Read<int>(exe.Slice(main2Ptr2 + main2Ptr + main1Ptr + constantPtr + 1));

                var main3Ptr = exe.Slice(main1Ptr + constantPtr + 2).FindPointerFromSignature(MOV_DWORD_LITERAL_SIGNATURE);
                if (main3Ptr == -1)
                {
                    Logger.Error(null, "Can't find main[3]");
                    return 4;
                }

                LogNumber("Main table MOV instruction 3", main3Ptr + main1Ptr + constantPtr + 2);
                main[2] = MemoryMarshal.Read<int>(exe.Slice(main3Ptr + main1Ptr + constantPtr + 4));

                var fencePtr = exe.Slice(main1Ptr + constantPtr).FindPointerFromSignature(FENCE_SIGNATURE);
                if (fencePtr == -1)
                {
                    Logger.Error(null, "Can't find fence");
                    return 4;
                }

                LogNumber("Fence IMUL instruction", fencePtr + main1Ptr + constantPtr);
                var fence = MemoryMarshal.Read<int>(exe.Slice(fencePtr + main1Ptr + constantPtr + 2));

                Console.WriteLine();

                Logger.Log24Bit(ConsoleSwatch.COLOR_RESET, null, false, Console.Out, null, "All table entries are prime? ");
                if (main.All(x => x.IsPrime()) && lengths.All(x => x.IsPrime()) && table.All(x => x.IsPrime()))
                {
                    Logger.Log24Bit(ConsoleSwatch.XTermColor.GreenYellow, false, Console.Out, null, "Yes");
                }
                else
                {
                    Logger.Log24Bit(ConsoleSwatch.XTermColor.Red, false, Console.Out, null, "No");
                }

                Console.WriteLine();

                LogNumberImportant("table[0]", table[0]);
                LogNumberImportant("table[1]", table[1]);
                LogNumberImportant("table[2]", table[2]);
                LogNumberImportant("lengths[0]", lengths[0]);
                LogNumberImportant("lengths[1]", lengths[1]);
                LogNumberImportant("lengths[2]", lengths[2]);
                LogNumberImportant("main[0]", main[0]);
                LogNumberImportant("main[1]", main[1]);
                LogNumberImportant("main[2]", main[2]);
                LogNumberImportant("fence", fence);

                Console.WriteLine();
            }

            return 0;
        }

        private static void LogNumber(string message, int number)
        {
            Logger.Log24Bit(ConsoleSwatch.COLOR_RESET, null, false, Console.Out, null, $"{message}: ");
            Logger.Log24Bit(ConsoleSwatch.XTermColor.Fuchsia, true, Console.Out, null, $"0x{number:X8}");
        }

        private static void LogNumberImportant(string message, int number)
        {
            Logger.Log24Bit(ConsoleSwatch.COLOR_RESET, null, false, Console.Out, null, $"{message}: ");
            Logger.Log24Bit(ConsoleSwatch.XTermColor.OrangeRed, true, Console.Out, null, $"0x{number:X2}");
        }
    }
}
