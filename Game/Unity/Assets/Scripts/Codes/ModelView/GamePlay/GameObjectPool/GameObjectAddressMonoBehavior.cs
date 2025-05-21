using System;
using UnityEngine;

namespace ET
{
    public class GameObjectAddressMonoBehavior : MonoBehaviour
    {
        public string Address { get; set; }
        public bool IsUIModule { get; set; }
        public GameObject Prefab { get; set; }


        public void Init(string path, bool isUIModule=false)
        {
            Address = path;
            IsUIModule = isUIModule;
        }
        
        public void Init(GameObject pre, bool isUIModule = false)
        {
            Prefab = pre;
            IsUIModule = isUIModule;
        }
        
    }
}