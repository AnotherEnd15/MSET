namespace ET.DBProxy;

[SceneController(SceneType.DBProxy)]
public class SceneController_DBProxy : ISceneController
{
    public void OnCreate(Scene scene, StartSceneConfig startSceneConfig)
    {
        var url = ServerConfig.Instance.Config["mongo_url"].ToString();
        scene.AddComponent<DBProxyComponent, string, string>(url, "db_"+ServerConfig.Instance.HostName);
        scene.AddComponent<DBVersionComponent>();
    }

    public async ETTask OnInit(Scene scene)
    {
        await scene.GetComponent<DBVersionComponent>().Init(1);
    }

    public async ETTask OnSave(Scene scene)
    {
        await ETTask.CompletedTask;
    }
}