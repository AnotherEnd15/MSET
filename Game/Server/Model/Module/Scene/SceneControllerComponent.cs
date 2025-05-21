using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class SceneControllerComponent : Entity,IAwake,IDestroy,ILoad
    {
        [StaticField]
        public static SceneControllerComponent Instance;
        public Dictionary<SceneType, List<ISceneController>> AllSceneControllers = new();
    }
}