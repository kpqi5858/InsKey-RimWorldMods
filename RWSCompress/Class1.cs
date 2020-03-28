using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;
using Ionic.Zlib;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace RWSCompressor
{
    public class MainMod : Mod
    {
        private static readonly char[] XMLHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>".ToCharArray();

        public static bool LogDetailed = false;

        public MainMod(ModContentPack content) : base(content)
        {
            var hinstance = new Harmony("RWSCompressor.Harmony");
            
            hinstance.PatchAll();
        }

        public static Dictionary<string, bool> CompressedCache = new Dictionary<string, bool>();

        public static bool IsCompressed(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                char[] Buffer = new char[XMLHeader.Length];

                reader.Read(Buffer, 0, XMLHeader.Length);
                return !Buffer.SequenceEqual(XMLHeader);
            }
        }

        public static bool IsCompressedCache(string path)
        {
            if (CompressedCache.ContainsKey(path))
            {
                return CompressedCache[path];
            }
            else
            {
                bool Val = IsCompressed(path);
                CompressedCache.Add(path, Val);
                return Val;
            }
        }

        public static void DecompressFile(string path)
        {
            string TmpFileLoc = path + ".dctmp";

            using (FileStream TempFile = new FileStream(TmpFileLoc, FileMode.Create))
            {
                using (Stream ReadStream = GetRightReadStream(path))
                {
                    ReadStream.CopyTo(TempFile);

                    //.NET 4 has CopyTo, so obsolete that

                    /*
                    //CopyTo implementation?
                    byte[] Buffer = new byte[32768];
                    int read;
                    while ((read = ReadStream.Read(Buffer, 0, Buffer.Length)) > 0)
                    {
                        TempFile.Write(Buffer, 0, read);
                    }
                    */
                }
            }

            File.Delete(path);
            File.Move(TmpFileLoc, path);
        }

        /// <summary>
        /// Checks file is compressed, and returns the correct stream for that file
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The stream</returns>
        public static Stream GetRightReadStream(string path)
        {
            bool Compressed = false;

            try
            {
                Compressed = IsCompressed(path);
            }
            catch (Exception e)
            {
                Log.ErrorOnce("Fatal error while checking file is compressed, assuming file is compressed : " + e, path.GetHashCode() ^ 1234152);
                Compressed = true;
            }

            Stream stream = null;
            try
            {
                if (Compressed)
                {
                    if (!IsSaveFile(path))
                    {
                        Log.Warning("Assumed compressed file which is not a save file : " + path);
                    }
                    stream = new DeflateStream(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None), CompressionMode.Decompress);
                }
                else
                {
                    stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                }
            }
            catch
            {
                throw;
            }

            return stream;
        }

        public static Stream GetRightWriteStream(string path)
        {
            //Only compress files that are save file
            if (IsSaveFile(path))
            {
                return new DeflateStream(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None), CompressionMode.Compress);
            }
            else
            {
                return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            }
        }

        public static bool IsSaveFile(string path)
        {
            const string SaveExt = GenFilePaths.SavedGameExtension;
            const string SaveExt_New = SaveExt + ".new";
            const string SaveExt_Old = SaveExt + ".old";

            return path.EndsWith(SaveExt) 
                || path.EndsWith(SaveExt_New)
                || path.EndsWith(SaveExt_Old);
        }
    }

    [HarmonyPatch(typeof(ScribeLoader), "InitLoading")]
    public class Patch_Loader1
    {
        /// <summary>
        /// This finds following c# code
        /// 
        /// using (StreamReader streamReader = new StreamReader(filePath))
        ///                                    --------------------------
        /// and replace to
        /// 
        /// using (StreamReader streamReader = new StreamReader(MainMod.GetRightReadStream()))
        ///                                    ----------------------------------------------
        /// </summary>
        /// <param name="instructions"></param>
        /// <param name="il"></param>
        /// <returns></returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var CodeList = instructions.ToList();

            //Find C# : using (StreamReader streamReader = new StreamReader(filePath))
            int NewObj = CodeList.FirstIndexOf((CodeInstruction inst) => inst.opcode == OpCodes.Newobj && inst.operand == typeof(StreamReader).GetConstructor(new Type[] { typeof(string) } ));

            //call MainMod.GetRightReadStream()
            CodeList[NewObj] = new CodeInstruction(OpCodes.Call, typeof(MainMod).GetMethod("GetRightReadStream"));

            //new StreamReader(MainMod.GetRightReadSteam())
            CodeList.Insert(NewObj+1, new CodeInstruction(OpCodes.Newobj, typeof(StreamReader).GetConstructor(new Type[] { typeof(Stream) })));
            return CodeList;
        }
    }

    [HarmonyPatch(typeof(ScribeLoader), "InitLoadingMetaHeaderOnly")]
    public class Patch_Loader2
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            return Patch_Loader1.Transpiler(instructions, il);
        }
    }

    [HarmonyPatch(typeof(ScribeMetaHeaderUtility), "GameVersionOf")]
    public class Patch_Loader3
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            return Patch_Loader1.Transpiler(instructions, il);
        }
    }

    [HarmonyPatch(typeof(ScribeSaver), "InitSaving")]
    public class Patch_Saver
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var CodeList = instructions.ToList();

            //Find C# : new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)
            int Target = CodeList.FirstIndexOf((CodeInstruction inst) => inst.opcode == OpCodes.Newobj && inst.operand == typeof(FileStream).GetConstructor(new Type[] { typeof(string), typeof(FileMode), typeof(FileAccess), typeof(FileShare) }));

            int ArgLen = 4;

            for (int i = Target - ArgLen; i < Target; i++)
            {
                CodeList[i] = new CodeInstruction(OpCodes.Nop);
            }

            //Replace to : MainMod.GetRightWriteStream(filePath)
            CodeList[Target - 1] = new CodeInstruction(OpCodes.Ldarg_1);
            CodeList[Target] = new CodeInstruction(OpCodes.Call, typeof(MainMod).GetMethod("GetRightWriteStream"));

            return CodeList;
        }
    }

    [HarmonyPatch(typeof(Dialog_FileList), "DrawDateAndVersion")]
    public class Patch_FileList
    {
        public static void Prefix(Dialog_FileList __instance, SaveFileInfo sfi, Rect rect)
        {
            Rect TargetRect = new Rect(rect.x - 100f, 4f, 93f, 30f);

            string filepath = sfi.FileInfo.FullName;

            if (MainMod.IsCompressedCache(filepath))
            {
                if (Widgets.ButtonText(TargetRect, "RC_Decompress".Translate()))
                {
                    try
                    {
                        MainMod.DecompressFile(filepath);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Fatal error while decompressing(restoring) file : " + e);
                    }
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    MainMod.CompressedCache.Clear();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_SaveFileList), "ReloadFiles")]
    public class Patch_ReloadFiles
    {
        public static void Prefix()
        {
            MainMod.CompressedCache.Clear();
        }
    }
}