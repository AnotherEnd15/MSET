namespace ET
{
    public class SceneControllerAttribute : BaseAttribute
    {
        public SceneType SceneType;
        
        public SceneControllerAttribute(SceneType sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}