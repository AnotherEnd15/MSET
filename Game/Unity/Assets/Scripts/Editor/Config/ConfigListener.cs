using System.IO;
using ET;
using UnityEditor;
using UnityEngine;

namespace ClientEditor
{
    public static class ConfigLisenter
    {
        static FileSystemWatcher watcher;
        static FileSystemEventHandler fileChangedCb;
        static RenamedEventHandler fileRenameCb;
 
        static FileSystemEventArgs curChangeEventArgs;
 
        static bool isInitialized = false;
        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (isInitialized) return;
            EditorApplication.update += OnUpdate;
            curChangeEventArgs = null;
            watcher = new FileSystemWatcher(Application.dataPath+"/Bundles/Config/", "*.*");
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            fileChangedCb = new FileSystemEventHandler(OnDataTableChanged);
            fileRenameCb = new RenamedEventHandler(OnDataTableChanged);
            watcher.Changed += fileChangedCb;
            watcher.Deleted += fileChangedCb;
            watcher.Renamed += fileRenameCb;
            isInitialized = true;
        }
 
        private static void OnUpdate()
        {
            if (curChangeEventArgs != null)
            {
                Debug.Log("Config文件夹变动,触发编辑器刷新");
                ET.EditorTool.Combine();
                AssetDatabase.Refresh();
                curChangeEventArgs = null;
            }
        }
 
        private static void OnDataTableChanged(object sender, FileSystemEventArgs e)
        {
            curChangeEventArgs = e;
        }
    }
}