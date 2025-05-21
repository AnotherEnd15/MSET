using ET.GamePlay;
using ET.Helper;
using ET.Robot;

namespace ET.TestCase
{
    [RobotTestCase(TestCaseId.Mining)]
    public class TestCase_Mining : IRobotTestCase
    {
        public async ETTask Run()
        {
            for (int i = 0; i < 100; i++)
            {
                Scene clientScene = await RobotHelper.CreateRobot(i+1);
                clientScene.GetOrAdd<ChatComponent>();
                await GMHelper.SendGM(clientScene, "UnlockAllModule");
                await GMHelper.SendGM(clientScene, "AddItem", ConstValue.PickaxeItemID.ToString(), "10000");
                await GMHelper.SendGM(clientScene, "AddItem", ConstValue.DrillItemID.ToString(), "10000");
                await GMHelper.SendGM(clientScene, "AddItem", ConstValue.BomItemID.ToString(), "10000");
                HandleMining(clientScene).Coroutine();
            }
        }

        async ETTask HandleMining(Scene clientScene)
        {
            List<int> tools=new List<int>() { ConstValue.PickaxeItemID , ConstValue.DrillItemID , ConstValue.BomItemID}; 
            while (true)
            {
                await TimerComponent.Instance.WaitAsync(200);
                try
                {
                    if (clientScene.IsDisposed)
                        return;
                    PlayerMineDataComponent mineCom=clientScene.GetComponent<PlayerMineDataComponent>();
                    var grids = mineCom.Grids.FindAll(grid => grid.IsCanOpen);
                    var exit = grids.Find(grid => grid.BaseConfigID == ConstValue.MineExit);
                    if (exit!=null)
                    {
                        await mineCom.OpenMine(exit.X, exit.Y, ConstValue.PickaxeItemID,true);
                    }
                    else
                    {
                        switch (tools.RandomArray())
                        {
                            case ConstValue.PickaxeItemID:
                                {//镐子
                                    grids = grids.FindAll(grid => !grid.IsOpen);
                                    var grid = grids.RandomArray();
                                    await mineCom.OpenMine(grid.X, grid.Y, ConstValue.PickaxeItemID, true);
                                }
                                break;
                            case ConstValue.DrillItemID:
                                {//钻头
                                    grids = grids.FindAll(grid => grid.IsOpen);
                                    var grid = grids.RandomArray();
                                    await mineCom.OpenMine(grid.X, grid.Y, ConstValue.DrillItemID, true);
                                }
                                break;
                            case ConstValue.BomItemID:
                                {//炸弹
                                    grids = grids.FindAll(grid => grid.IsOpen);
                                    var grid = grids.RandomArray();
                                    await mineCom.OpenMine(grid.X, grid.Y, ConstValue.BomItemID, true);
                                }
                                break;

                        }
                    }
                }
                catch (Exception e)
                {
                    Log.GetLogger().Error(e);
                }
            }
        }
    }
}