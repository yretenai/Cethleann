using System.IO;
using System.Linq;
using Cethleann.Ninja;
using DragonLib;
using DragonLib.IO;

namespace Dissidia.SaveDecrypt
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Logger.Error("DFFNT", "Usage: Dissidia.SaveDecrypt.exe user_storage.dat key");
                return;
            }

            var file = args[0];
            var key = args[1].ToHexOctetsB();
            var save = new DFFNTSave(File.ReadAllBytes(file), key);
            File.WriteAllText(Path.ChangeExtension(file, ".json"), save.SaveData);
            File.WriteAllText(Path.ChangeExtension(file, ".txt"), string.Join('\n', (new ulong[] { 1, save.Header.Version, save.Header.Reserved1, save.Header.Reserved2, save.Header.Unknown3, save.Header.Unknown4, save.TailByte }).Select(x => x.ToString("X"))));
            File.WriteAllBytes(Path.ChangeExtension(file, ".dec"), save.Write().ToArray());
        }
    }
}
