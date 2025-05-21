using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace ET
{

    public static partial class YooAssetsHelper
    {
        public static async ETTask Init()
        {
            YooAssets.Initialize();
            var package = YooAssets.CreatePackage("DefaultPackage");
            YooAssets.SetDefaultPackage(package);

            InitializeParameters initParameters = null;
            
#if UNITY_EDITOR
            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");    
            var packageRoot = buildResult.PackageRootDirectory;
            var initParams = new EditorSimulateModeParameters();
            initParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
#else
            throw new Exception("Demo中没有非编辑器模式的资源加载 请自行实现")
#endif


            Log.ImportantInfo($"YooAsset 开始初始化 {GameConst.GameVersion}");
            initParameters = initParams;

            var initOpera = package.InitializeAsync(initParameters);
            await initOpera.Task;
            if (initOpera.Status != EOperationStatus.Succeed)
            {
                //todo: 弹窗提示
                Log.Error($"资源包初始化失败：{initOpera.Error}");
                Application.Quit();
                return;
            }
        }
    }
}