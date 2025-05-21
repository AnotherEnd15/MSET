namespace ET
{
    public abstract class AUIEvent
    {
        public abstract ETTask<UI> OnCreate(UI parentUI,UILayerType layerType);
        public abstract void OnRemove(Scene scene);
    }
}