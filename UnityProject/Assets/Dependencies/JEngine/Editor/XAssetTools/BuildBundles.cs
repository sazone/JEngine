#if !XASSET_6
using System;
using System.Diagnostics;
using JEngine.Core;
using libx;
using UnityEditor;

namespace JEngine.Editor
{
    public class BuildBundles
    {
        [MenuItem("JEngine/XAsset/Bundles/Build Bundles %#&B")]
        private static void BuildAssetBundles()
        {
            DLLMgr.Delete("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
            // DLLMgr.Delete(Directory.GetParent(Application.dataPath)+"/Assets/XAsset/ScriptableObjects/Rules.asset");
            // DLLMgr.Delete(Directory.GetParent(Application.dataPath)+"/Assets/XAsset/ScriptableObjects/Manifest.asset");


            CryptoWindow.ShowWindow();
            CryptoWindow.Build= s =>
            {
                var watch = new Stopwatch();
                watch.Start();
                var bytes = DLLMgr.FileToByte(DLLMgr.DllPath);
                var result = DLLMgr.ByteToFile(CryptoHelper.AesEncrypt(bytes, s),
                    "Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
                watch.Stop();
                Log.Print(String.Format(Setting.GetString(SettingString.DLLConvertLog),
                    watch.ElapsedMilliseconds));
                if (!result)
                {
                    Log.PrintError(".dll加密转.bytes出错！");
                }
            
                watch = new Stopwatch();
                watch.Start();
                BuildScript.ApplyBuildRules();
                watch.Stop();
                Log.Print("ApplyBuildRules in: " + watch.ElapsedMilliseconds + " ms.");
            
                watch = new Stopwatch();
                watch.Start();
                BuildScript.BuildAssetBundles();
                watch.Stop();
                Log.Print("BuildAssetBundles in: " + watch.ElapsedMilliseconds + " ms."); 
            };
        }
    }
}
#endif