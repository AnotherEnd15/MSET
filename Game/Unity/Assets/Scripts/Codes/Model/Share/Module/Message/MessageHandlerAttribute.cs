namespace ET
{
    public class MessageHandlerAttribute : BaseAttribute
    {
        public SceneType SceneType;

#if DOTNET && !ROBOT
        public MessageHandlerAttribute(SceneType sceneType)
        {
            this.SceneType = sceneType;
        }
#else
        public MessageHandlerAttribute() // 客户端不需要填SceneType
        {

        }
#endif
    }
}