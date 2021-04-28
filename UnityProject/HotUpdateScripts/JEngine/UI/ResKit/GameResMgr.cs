//
// GameResMgr.cs
//
// Author:
//       L-Fone <275757115@qq.com>
//
// Copyright (c) 2020 JEngine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using JEngine.Core;
using VEngine;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JEngine.UI.ResKit
{
    public class GameResMgr
    {
        /// <summary> 资源分类策略，分类型存储,key = 加载类型,value = 资源 </summary>
        Dictionary<ResType, Dictionary<string, Asset>> _gameDic = null;

        Dictionary<string, Action<Object>> _gameActionDic = null;

        public GameResMgr()
        {
            _gameDic = new Dictionary<ResType, Dictionary<string, Asset>>();
            foreach (ResType type in Enum.GetValues(typeof(ResType)))
                _gameDic.Add(type, new Dictionary<string, Asset>());

            _gameActionDic = new Dictionary<string, Action<Object>>();
        }


        //加载场景的进度
        public static float LoadSceneProgress = 0;

        public bool IsExistRes(ResType type, string name) { return _gameDic[type].ContainsKey(name); }

        public void AddAssetRequest(Asset req, string name, ResType loadType)
        {
            if (!_gameDic.ContainsKey(loadType))
                _gameDic.Add(loadType, new Dictionary<string, Asset>());
            if (!_gameDic[loadType].ContainsKey(name))
                _gameDic[loadType].Add(name, req);
        }

        public Asset GetAssetRequest(string resName, ResType loadType)
        {
            if (string.IsNullOrEmpty(resName)) return null;
            Asset req = null;
            Dictionary<string, Asset> _dic = null;
            _gameDic.TryGetValue(loadType, out _dic);
            if (_dic == null || _dic.Count == 0) return req;
            _dic.TryGetValue(resName, out req);
            return req;
        }

        public Object GetObject(ResType type, string name)
        {
            Asset req = GetAssetRequest(name, type);
            return req != null ? req.asset : null;
        }

        #region 资源加载策略
        /// <summary>
        /// 同步加载资源
        /// </summary>
        public T LoadAsset<T>(ResType loadType, string name) where T : UnityEngine.Object
        {
            Asset res = GetAssetRequest(name, loadType);
            if (res == null)
            {
                AddAssetRequest(res, name, loadType);
                res = Asset.Load(name, typeof(T));
            }
            return res.asset as T;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void LoadAssetAsync<T>(ResType loadType, string name, AssetType assetType, Action<Object> callback) where T : UnityEngine.Object
        {
            Asset res = GetAssetRequest(name, loadType);
            if (res != null)
            {
                callback?.Invoke(res.asset as T);
                return;
            }

            if (_gameActionDic.ContainsKey(name))
            {
                _gameActionDic[name] += callback;
            }
            else
            {
                _gameActionDic.Add(name, callback);
                res = Asset.LoadAsync(ResPath.Instance.GetPath(name, assetType), typeof(T));
                Action<Asset> call = (resource) =>
                {
                    AddAssetRequest(res, name, loadType);
                    //callback?.Invoke(resource.asset as T);
                    if (_gameActionDic.ContainsKey(name))
                    {
                        _gameActionDic[name]?.Invoke(resource.asset as T);
                        _gameActionDic.Remove(name);
                    }
                };
                if (res.isDone)
                {
                    call.Invoke(res);
                }
                res.completed += call;
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path">场景名称</param>
        /// <param name="callback">完成回调</param>
        /// <param name="additive">是否叠加在现有场景上</param>
        public async void LoadSceneAsync(string path, Action callback = null, bool additive = false)
        {
            var req = Scene.LoadAsync(path,null, additive);
            req.completed += delegate
            {
                callback?.Invoke();
                LoadSceneProgress = 1;
            };
            while (!req.isDone)
            {
                LoadSceneProgress = req.progress;
                await System.Threading.Tasks.Task.Delay(1);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void UnloadAsset(ResType type, string name)
        {
            if (_gameDic[type].ContainsKey(name))
            {
                _gameDic[type][name].Release();
                _gameDic[type].Remove(name);
            }
        }

        /// <summary>
        /// 释放整个类别的资源
        /// </summary>
        public void UnloadAsset(ResType type)
        {
            foreach (Asset req in _gameDic[type].Values)
            {
                req.Release();
            }
            _gameDic[type].Clear();
        }

        /// <summary>
        /// 释放一组资源
        /// </summary>
        public void UnloadAsset(ResType type, List<string> names)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (_gameDic[type].ContainsKey(names[i]))
                {
                    _gameDic[type][names[i]].Release();
                    _gameDic[type].Remove(names[i]);
                }
            }
        }

        /// <summary>
        /// 卸载所有未引用到的资源
        /// </summary>
        public void RemoveUnusedAssets()
        {
            //6.1不需要这个
        }
        #endregion
    }
}
