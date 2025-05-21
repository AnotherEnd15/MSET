using ET.Helper;
using ET.Robot;

namespace ET.TestCase
{
    [RobotTestCase(TestCaseId.PetSummon)]
    public class TestCase_PetSummon : IRobotTestCase
    {
        public async ETTask Run()
        {
            for (int i = 0; i < 100; i++)
            {
                Scene clientScene = await RobotHelper.CreateRobot(i+1);

                await GMHelper.SendGM(clientScene, "UnlockAllModule");
                HandleChat(clientScene).Coroutine();
            }
        }

        async ETTask HandleChat(Scene clientScene)
        {
            var configs = ShopSummonConfigCategory.Instance.DataList.FindAll(config => !config.ADType.HasValue
            && config.ItemType == ItemType.Pet);
            while (true)
            {
                await TimerComponent.Instance.WaitAsync(200);
                try
                {
                    if (clientScene.IsDisposed)
                        return;

                    var req = new C2M_ShopSummonBuyRequest()
                    {
                        ConfigId = configs.RandomArray().Id
                    };
                    var resp = await clientScene.GateSession().Call(req);

                }
                catch (Exception e)
                {
                    Log.GetLogger().Error(e);
                }
            }
        }
    }
}