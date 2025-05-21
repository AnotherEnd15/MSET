using ET.Helper;
using ET.Robot;

namespace ET.TestCase
{
    [RobotTestCase(TestCaseId.AddMail)]
    public class TestCase_Mail : IRobotTestCase
    {
        public async ETTask Run()
        {
            for (int i = 0; i < 20; i++)
            {
                Scene clientScene = await RobotHelper.CreateRobot(i + 1);
                clientScene.GetOrAdd<ChatComponent>();
                await GMHelper.SendGM(clientScene, "UnlockAllModule");
                HandleMail(clientScene).Coroutine();
            }
        }

        async ETTask HandleMail(Scene clientScene)
        {
            while (true)
            {
                await GMHelper.SendGM(clientScene, "AddMail","0",ItemConfigCategory.Instance.DataList.RandomArray().Id.ToString(),"1");
                await TimerComponent.Instance.WaitAsync(3000);
            }
        }
    }
}