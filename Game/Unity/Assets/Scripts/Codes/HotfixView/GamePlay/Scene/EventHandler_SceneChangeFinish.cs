using System;
using ET.EventType;
using UnityEngine;

namespace ET
{
    [Event]
    public class EventHandler_SceneChangeFinish : AEvent<Scene,SceneChangeFinish>
    {
        protected override async ETTask Run(Scene currentScene, SceneChangeFinish a)
        {
            var clientScene = currentScene.ClientScene();

            if (currentScene.IsDisposed)
                return;

        }


    }
}