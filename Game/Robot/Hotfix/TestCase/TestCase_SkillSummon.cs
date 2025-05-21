using ET.Helper;
using ET.Robot;
using ET.Shop;

namespace ET.TestCase
{
    [RobotTestCase(TestCaseId.SkillSummon)]
    public class TestCase_SkillSummon : IRobotTestCase
    {
        public async ETTask Run()
        {
            for (int i = 0; i < 100; i++)
            {
                Scene clientScene = await RobotHelper.CreateRobot(i+1);
                clientScene.GetOrAdd<ChatComponent>();
                await GMHelper.SendGM(clientScene, "UnlockAllModule");
                HandleSkill(clientScene).Coroutine();
            }
        }

        async ETTask HandleSkill(Scene clientScene)
        {
            var configs = ShopSummonConfigCategory.Instance.DataList.FindAll(config => !config.ADType.HasValue
            && config.ItemType == ItemType.Spell);
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