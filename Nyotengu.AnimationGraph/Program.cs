using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cethleann.Archive;
using Cethleann.KTID;
using Cethleann.Structure.KTID;
using DragonLib.CLI;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Nyotengu.AnimationGraph
{
    [PublicAPI]
    public static class Program
    {
        private static void Main(string[] args)
        {
            Logger.PrintVersion("Nyotengu");
            var flags = CommandLineFlags.ParseFlags<AnimationGraphFlags>(CommandLineFlags.PrintHelp, args);
            if (flags == null) return;

            var objdbNdb = new NDB();
            var singletonNdb = new NDB();
            if (!string.IsNullOrEmpty(flags.OBJDBNDBPath) && File.Exists(flags.OBJDBNDBPath)) objdbNdb = new NDB(File.ReadAllBytes(flags.OBJDBNDBPath));
            if (!string.IsNullOrEmpty(flags.SingletonDBNDBPath) && File.Exists(flags.SingletonDBNDBPath)) singletonNdb = new NDB(File.ReadAllBytes(flags.SingletonDBNDBPath));

            var CE1DB = new OBJDB(File.ReadAllBytes(flags.OBJDBPath));
            var CE1Singleton = new OBJDB(File.ReadAllBytes(flags.SingletonPath));
            var filelist = Cethleann.ManagedFS.Nyotengu.LoadKTIDFileList(flags.FileList, flags.GameId);
            var animationFiles = new Dictionary<KTIDReference, string>();
            foreach (var directory in flags.AnimationDirectories ?? new HashSet<string?>())
            {
                if (directory == null || !Directory.Exists(directory)) continue;
                foreach (var file in Directory.GetFiles(directory))
                {
                    var basename = Path.GetFileNameWithoutExtension(file);
                    if (basename.Length != 8 || !KTIDReference.TryParse(basename, out var reference)) reference = RDB.Hash(file, "G1A");

                    animationFiles[reference] = file;
                }
            }

            var typeKTID = RDB.Hash("TypeInfo::Object::MotorCharacterSetting");
            foreach (var entry in CE1DB.Entries.Select(x => x.Value).Where(entry => entry.Record.TypeInfoKTID == typeKTID))
            {
                var (_, values) = entry.GetProperty("CharacterModelNameHash");
                if (values == null) continue;
                var (_, actions) = entry.GetProperty("CharacterActionObjectNameHashArray");
                if (actions == null || actions.Length == 0) continue;
                var nameHashes = values.Where(x => x != null).Select(x => new KTIDReference(x)).ToArray();
                if (!nameHashes.Any(x => flags.Hashes.Contains(x))) continue;
                Logger.Info("ANIM", $"Found Character Settings: {string.Join(", ", nameHashes.Select(x => GetKTIDNameValue(x, false, objdbNdb, filelist)))}");
                var done = new HashSet<KTIDReference>();
                foreach (var actionHash in actions.Select(x => new KTIDReference(x)))
                {
                    if (!CE1Singleton.Entries.TryGetValue(actionHash, out var player))
                    {
                        Logger.Error("ANIM", $"Can't find animation player settings for {GetKTIDNameValue(actionHash, false, singletonNdb, filelist)}");
                        continue;
                    }

                    var properties = player.GetProperties("AnimationDataObjectNameHashArray", "SrcAnimationDataObjectNameHash", "DstAnimationDataObjectNameHash", "FCurveAnimationDataObjectNameHash");
                    var animationDataHashes = properties.SelectMany(x => x.values ?? new object?[0]).ToArray();
                    var ktidHashes = animationDataHashes.Where(x => x != null).Select(x => new KTIDReference(x)).ToArray();
                    foreach (var animationDataHash in ktidHashes)
                    {
                        if (!CE1Singleton.Entries.TryGetValue(animationDataHash, out var animationData))
                        {
                            Logger.Error("ANIM", $"Can't find animation data for {GetKTIDNameValue(animationDataHash, false, singletonNdb, filelist)}");
                            continue;
                        }

                        var (_, animationHashes) = animationData.GetProperty("G1AFileResourceHash");
                        if (animationHashes == null)
                        {
                            Logger.Error("ANIM", $"Can't find animation references for {GetKTIDNameValue(animationDataHash, false, singletonNdb, filelist)}");
                            continue;
                        }

                        foreach (var animationHashActual in animationHashes.Where(x => x != null).Select(x => new KTIDReference(x)))
                        {
                            if (!done.Add(animationHashActual)) continue;
                            Logger.Info("ANIM", GetKTIDNameValue(animationHashActual, false, singletonNdb, filelist));

                            if (string.IsNullOrWhiteSpace(flags.Output) || !animationFiles.TryGetValue(animationHashActual, out var path)) continue;

                            if (!Directory.Exists(flags.Output)) Directory.CreateDirectory(flags.Output);
                            File.Copy(path, Path.Combine(flags.Output, Path.GetFileName(path)), true);
                        }
                    }
                }

                Console.WriteLine();
            }
        }

        private static string GetKTIDNameValue(KTIDReference ktid, bool ignoreNames, NDB ndb, params Dictionary<KTIDReference, string>[] filelists)
        {
            var name = $"{ktid:x8}";
            return ignoreNames ? name : $"{ktid.GetName(ndb, filelists) ?? name}";
        }
    }
}
