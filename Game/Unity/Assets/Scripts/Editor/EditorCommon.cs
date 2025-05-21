#if UNITY_EDITOR
using ET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{ 
public class EditorCommon
{
    public const string attDir = "Assets/Bundles/Config";
    public static T LoadConfig<T>() where T : CategoryBase
    {
        string path = attDir + "/" + typeof(T).Name + ".bytes";
        UnityEngine.TextAsset textas = AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(path);
        try
        {
            if (textas != null)
            {
                    var instance = Activator.CreateInstance(typeof(T)) as CategoryBase;
                    instance.Load(new Luban.ByteBuf(textas.bytes));
                    return instance as T;
            }
            else
            {
                return Activator.CreateInstance(typeof(T)) as T;
            }
        }
        catch (Exception e)
        {
            throw new Exception($"parser json fail: {path}", e);
        }


    }
        public static T SaveConfig<T>(T configCategory) where T : CategoryBase
        {
            string path = attDir + "/" + typeof(T).Name + ".bytes";
            UnityEngine.TextAsset textas = AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(path);
            try
            {
                if (textas != null)
                {
                    var instance = Activator.CreateInstance(typeof(T)) as CategoryBase;
                    instance.Load(new Luban.ByteBuf(textas.bytes));
                    return instance as T;
                }
                else
                {
                    return Activator.CreateInstance(typeof(T)) as T;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"parser json fail: {path}", e);
            }


        }

    }
}
#endif