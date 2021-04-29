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
                VEngine.Editor.BuildScript.beforeBuildBundles = (build) =>
                {
                    DLLMgr.Delete("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
                    CryptoWindow.ShowWindow();
                    CryptoWindow.Build = s =>
                    {
                        var watch = new Stopwatch();
                        watch.Start();
                        var bytes = DLLMgr.FileToByte(DLLMgr.DllPath);
                        var b = DLLMgr.ByteToFile(CryptoHelper.AesEncrypt(bytes, s),
                            "Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
                        watch.Stop();
                        Log.Print(String.Format(Setting.GetString(SettingString.DLLConvertLog),
                            watch.ElapsedMilliseconds));
                        if (!b)
                        {
                            Log.PrintError(".dll加密转.bytes出错！");
                            return;
                        }
                        build();
                    };
                };
                Log.Print(1);
                _hookedBuildEvents = true;
            }
            
            if (!Setting.XAssetLoggedIn || _delaying || _verifying || XAssetHelper.loggingXAsset) return;

            //验证
            _verifying = true;
            var result = await XAssetHelper.LoginXAsset();
            _verifying = false;
            
            if (!result)
            {
                XAssetHelper.LogOutXAsset();
                EditorUtility.DisplayDialog("XAsset", "登入状态异常，请重新登入\n" +
                                                      "An error just occured, please log in again", "OK");
                Setting.Refresh();
            }
            _delaying = true;
            await Task.Delay(TimeSpan.FromSeconds(_frequency));
            _delaying = false;
        }
    }
}