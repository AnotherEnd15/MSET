using ET.Helper;
using ET.Robot;

namespace ET.TestCase
{
    [RobotTestCase(TestCaseId.Default)]
    public class TestCase_Default : IRobotTestCase
    {
        public async ETTask Run()
        {
            for (int i = 0; i < 3000; i++)
            {
                Scene clientScene = await RobotHelper.CreateRobot(i+1);
                await GMHelper.SendGM(clientScene, "AddItem", "3", "10000");
            }
        }
    }
}