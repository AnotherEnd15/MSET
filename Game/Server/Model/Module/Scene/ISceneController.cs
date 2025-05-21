
namespace ET
{
    // Scene上添加的模块需要实现的 可以处理1个或者多个组件的添加/初始化处理
    public interface ISceneController
    {
        // 非初始Scene的startSceneConfig是空的
        void OnCreate(Scene scene, StartSceneConfig startSceneConfig);

        // 当不同进程的所有Scene的OnCreate走完 才会触发
        // 不同进程的 1区的Scene的OnInit全部执行完毕后 再执行游戏区的
        ETTask OnInit(Scene scene);

        // 每个Scene大约15分钟左右触发一次保存
        // 只有父物体是ServerSceneManagerComponent.Instance 也就是初始创建的Scene才会触发
        // 次级Scene 如果想要在父Scene保存时跟随保存 比如MapCenter中动态创建的Map
        // 需要在MapCenter OnSave时中手动调用所有Map的OnSave (目前没有这样做)
        ETTask OnSave(Scene scene);
    }
}