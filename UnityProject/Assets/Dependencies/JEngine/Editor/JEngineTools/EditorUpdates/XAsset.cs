using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JEngine.Core;
using UnityEditor;

namespace JEngine.Editor
{
    internal static class XAsset
    {
        public static bool hasAdded;
        private static bool _hookedBuildEvents;
        private static bool _delaying;
        private static bool _verifying;
        private static float _frequency = 3600;

        public static async void Update()
        {
            hasAdded = true;
            
            //Hook
            if (!_hookedBuildEvents)
            {
                bool hasConvert = false;
                VEngine.Editor.BuildScript.preprocessBuildBundles = (manifest) =>
                {
                    if (hasConvert) return;
                    var path = "Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes";
                    DLLMgr.Delete(path);
                    var key = UnityEngine.Object.FindObjectOfType<InitJEngine>().key;
                    var watch = new Stopwatch();
                    watch.Start();
                    var bytes = DLLMgr.FileToByte(DLLMgr.DllPath);
                    var b = DLLMgr.ByteToFile(CryptoHelper.AesEncrypt(bytes, key), path);
                    watch.Stop();
                    Log.Print(String.Format(Setting.GetString(SettingString.DLLConvertLog),
                        watch.ElapsedMilliseconds));
                    if (!b)
                    {
                        Log.PrintError(".dll加密转.bytes出错！");
                        return;
                    }
                    hasConvert = true;
                };
                VEngine.Editor.BuildScript.postprocessBuildBundles = (manifest) =>
                {
                    hasConvert = true;
                };
                _hookedBuildEvents = true;
            }
        }
    }
}