using System;
using System.Collections;
using System.IO;
using System.Text;
using JEngine.Core;
using UnityEngine;
using UnityEngine.UI;
using VEngine;
using Logger = VEngine.Logger;

public class Initializer : MonoBehaviour
{
    public string url;
    public string nextScene;
    
    public bool Initialized { get; private set; }
    
    public enum LoadMode
    {
        LoadByName,
        LoadByNameWithoutExtension,
    }
    public string[] searchablePaths = new []
    {
        "Assets/HotUpdateResources"
    };

    /// <summary>
    ///     加载模式，目前支持两种：LoadByName 使用扩展名，LoadByNameWithoutExtension 不使用扩展名
    /// </summary>
    [Tooltip("加载模式")] public LoadMode loadMode = LoadMode.LoadByName;

    /// <summary>
    ///     初始化的清单配置，可以配置包内和包外的清单，底层会自动按需更新下载
    /// </summary>
    [Tooltip("初始化的清单配置，可以配置包内和包外的清单，底层会自动按需更新下载")]
    public string[] manifests =
    {
        "HotResources"
    };

    public Slider progressBar;
    public Text progressText;
    
    private void Awake()
    {
        switch (loadMode)
        {
            case LoadMode.LoadByName:
                Versions.customLoadPath = LoadByName;
                break;
            case LoadMode.LoadByNameWithoutExtension:
                Versions.customLoadPath = LoadByNameWithoutExtension;
                break;
            default:
                Versions.customLoadPath = null;
                break;
        }
    }

    public void StartUpdate()
    {
        StartCoroutine(DoUpdate());
    }

    IEnumerator DoUpdate()
    {
        DontDestroyOnLoad(gameObject);

        //对一些数据赋值
        Versions.DownloadURL = url;
        Logger.Loggable = GameStats.Debug;
        
        var operation = Versions.InitializeAsync();
        yield return operation;
        if (operation.status == OperationStatus.Failed)
        {
            // 如果初始化失败，可以通过错误日志分析原因。
            Logger.E("Failed to initialize Runtime with error: {0}", operation.error);
            //不yield break，增量加载会出错，但错误可忽略
        }

        var builder = new StringBuilder();
        builder.AppendFormat($"API Version: {Versions.APIVersion}, ");
        builder.AppendFormat($"Manifest Version: {Versions.ManifestsVersion}, ");
        builder.AppendFormat($"PlayerDataPath: {Versions.PlayerDataPath}, ");
        builder.AppendFormat($"DownloadDataPath: {Versions.DownloadDataPath}, ");
        builder.AppendFormat($"DownloadURL: {Versions.DownloadURL}.");
        Log.Print(string.Format("Success to initialize Runtime with: {0}", builder));
        
        //Init后开始Update
        Initialized = true;
        
        if (!Initialized)
        {
            Logger.E("Has not Initialized!");
        }
        
        //更新全部
        var update = Versions.UpdateAsync(manifests);
        yield return update;
        if (update.status == OperationStatus.Failed)
        {
            Logger.E("Update failed");
            yield break;
        }
        
        var check = Versions.GetDownloadSizeAsync("Bundled","Auto","Preload");
        yield return check;
        // 判断是否有内容需要更新
        if (check.result.Count > 0)
        {
            // 获取需要更新的内容大小
            var totalSize = Utility.FormatBytes(check.totalSize);
            Logger.I("{0}({1}) count files has being updated.", check.result.Count, totalSize);
            // 这里省略了请求下载的逻辑，直接启动更新 
            var download =  Versions.DownloadAsync(check.result.ToArray());
            // 采样时间，推荐每秒采样一次
            const float sampleTime = 0.1f;
            // 上次采样的进度，用来计算下载速度
            var lastDownloadedBytes = 0UL;
            var lastSampleTime = 0f;
            while (!download.isDone)
            {
                if (Time.realtimeSinceStartup - lastSampleTime > sampleTime)
                {
                    // 获取已经下载的内容大小
                    var progress = Utility.FormatBytes(download.downloadedBytes);
                    // 获取总大小
                    var max = Utility.FormatBytes(download.totalSize);
                    // 计算速度
                    var amount = lastDownloadedBytes - download.downloadedBytes;
                    var speed = Utility.FormatBytes((ulong) (amount / sampleTime));
                    progressBar.maxValue = download.totalSize;
                    progressBar.value = download.downloadedBytes;
                    progressText.text = string.Format("下载中.... {0}/{1}({2})", progress, max, speed);
                    lastDownloadedBytes = download.downloadedBytes;
                    lastSampleTime = Time.realtimeSinceStartup;
                }
                yield return null;
            }
            progressBar.maxValue = download.totalSize;
            progressBar.value = download.totalSize;
            progressText.text = "下载完成，准备跳转场景";
            yield return new WaitForSeconds(0.5f);//等0.5秒

        }

        Logger.I("Update finish with {0} files.", check.result.Count);

        //跳转场景，激活C#热更
        yield return Scene.LoadAsync(nextScene, scene =>
        {
            Logger.I("{0}:{1}", scene.status, scene.pathOrURL);
            FindObjectOfType<InitJEngine>().Load();
            ClassBindMgr.Instantiate();
            FindObjectOfType<InitJEngine>().OnHotFixLoaded();
        }); 
    }
    
    
    private string LoadByNameWithoutExtension(string assetPath)
    {
        if (searchablePaths == null || searchablePaths.Length == 0)
        {
            return null;
        }

        if (!Array.Exists(searchablePaths, assetPath.Contains)) return null;
        var assetName = Path.GetFileNameWithoutExtension(assetPath);
        return assetName;
    }

    private string LoadByName(string assetPath)
    {
        if (searchablePaths == null || searchablePaths.Length == 0)
        {
            return null;
        }

        if (!Array.Exists(searchablePaths, assetPath.Contains)) return null;
        var assetName = Path.GetFileName(assetPath);
        return assetName;
    }
}
