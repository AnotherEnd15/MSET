#if !UNITY_WEBGL
using System.IO;
//using ProtoBuf;
#endif
using System;
using System.IO;
using UnityEngine;

namespace ET
{
    public abstract class StorageSingleton<T>  where T : StorageSingleton<T>, new()
    {
        [StaticField]
        private static T instance = null;

        public static T Instance
        {
            get
            {
                instance ??= Load();
                return instance;
            }
        }
        protected abstract void AfterLoad();
        private static T Load()
        {
#if UNITY_WEBGL
            string content = PlayerPrefs.GetString($"StorageSingleton_{typeof(T).Name}");
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    T t = Deserialize(content.ToByteArrayBySplit()); //JsonUtility.FromJson<T>(content);
                    t.AfterLoad();
                    return t;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
#else
            string pth = Application.persistentDataPath + "/" + typeof(T).Name;
            Debug.Log("存档路径:" + pth);

            if (File.Exists(pth))
            {
                byte[] bytes = File.ReadAllBytes(pth);
                if (bytes != null)
                {
                    return Deserialize(bytes);
                }
            }
#endif
            return new T();
        }

        public virtual void Save()
        {
#if UNITY_WEBGL
            try
            {
                var content = this.Serialize();
                string contentStr = content.BytesToStringBySplit();
                PlayerPrefs.SetString($"StorageSingleton_{typeof(T).Name}", contentStr);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
#else
            string pth = Application.persistentDataPath + "/" + typeof(T).Name;
            if (!File.Exists(pth))
            {
                StreamWriter sw = File.CreateText(pth);
                sw.Close();
                sw.Dispose();
            }
            System.IO.File.WriteAllBytes(pth, Serialize());
#endif
        }

        #region 使用MemoryPack代替Json

        public byte[] Serialize()
        {
            return MemoryPackHelper.Serialize(this);

        }

        public static T Deserialize(byte[] datas)
        {
            return MemoryPackHelper.Deserialize(typeof(T), datas,0,datas.Length) as T;
        }

        #endregion
    }
}
