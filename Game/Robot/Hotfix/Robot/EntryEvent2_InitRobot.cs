using ET.EventType;

namespace ET.Robot
{
    [Event]
    public class EntryEvent2_InitRobot : AEvent<Scene,ET.EventType.EntryEvent2>
    {
        protected override async ETTask Run(Scene scene, EntryEvent2 a)
        {
           
            Game.AddSingleton<ConfigComponent>().LoadAll();
            ConfigComponent.Instance.RunConfigCheck();
            Root.Instance.Scene.AddComponent<RobotTestCaseComponent>();
            Root.Instance.Scene.AddComponent<ConsoleComponent>();
        }
    }
}