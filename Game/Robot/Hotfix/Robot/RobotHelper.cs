namespace ET.Robot
{
    public static class RobotHelper
    {
        public static async ETTask<Scene> CreateRobot(int zone)
        {
            var account = $"Robot_{zone}";
            Scene clientScene = SceneFactory.CreateClientScene(zone, account);

            var loginRet = await LoginHelper.LoginRealm(clientScene, account, false,"");
            var targetZone = loginRet.Item1.RecommendZone;
            var gateRet = await LoginHelper.LoginGate(clientScene, targetZone,new Proto_DeviceInfo()
            {
                Channel = "robot",
                DistId = $"robot{zone}",
                OS = "robot",
                PackageName = "robot",
                Version = "robot"
            });

            return clientScene;
        }
    }
}