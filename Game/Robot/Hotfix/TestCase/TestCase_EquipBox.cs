using ET.Helper;
using ET.Robot;

namespace ET.TestCase
{
    [RobotTestCase(TestCaseId.EquipBox)]
    public class TestCase_EquipBox : IRobotTestCase
    {
        public async ETTask Run()
        {
            for (int i = 0; i < 100; i++)
            {
                Scene clientScene = await RobotHelper.CreateRobot(i+1);
                await GMHelper.SendGM(clientScene, "AddItem", ConstValue.EquipBoxItemConfigId.ToString(), "999990");
                HandleEquipBox(clientScene).Coroutine();
            }
        }

        async ETTask HandleEquipBox(Scene clientScene)
        {
            while (true)
            {
                await TimerComponent.Instance.WaitAsync(RandomGenerator.RandomNumber(100,500));
                
                var gateSession = clientScene.GateSession();
                var equipCom = clientScene.GetComponent<EquipComponent>();
                var req = new C2M_AutoOpenEquipBoxRequest();
                var resp = (C2M_AutoOpenEquipBoxResponse)await gateSession.Call(req);
                if (resp.Error != 0)
                {
                    return;
                }

                if (equipCom.GetNeedHandleEquipCount() > 0)
                {
                    var sellReq = new C2M_EquipDeleteRequest();
                    var sellResp = (C2M_EquipDeleteResponse)await gateSession.Call(sellReq);
                }
            }
        }
    }
}