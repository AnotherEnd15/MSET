
namespace ET
{
    [Event]
    public class EntryEvent_InitClient: AEvent<Scene,ET.EventType.LogicEntryEvent>
    {
        protected override async ETTask Run(Scene scene, ET.EventType.LogicEntryEvent args)
        {
            Root.Instance.Scene.AddComponent<NetThreadComponent>();
            Root.Instance.Scene.AddComponent<OpcodeTypeComponent>();
            Root.Instance.Scene.AddComponent<MessageDispatcherComponent>();
            Root.Instance.Scene.AddComponent<NumericWatcherComponent>();
            Root.Instance.Scene.AddComponent<ClientSceneManagerComponent>();
            Root.Instance.Scene.AddComponent<AIManagerComponent>();
            Root.Instance.Scene.AddComponent<ConditionMgrComponent>();
            Root.Instance.Scene.AddComponent<StateMachineComponent>();
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
        }
    }
}