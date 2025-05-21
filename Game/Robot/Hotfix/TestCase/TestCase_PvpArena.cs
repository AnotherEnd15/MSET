using ET.Chat;
using ET.EventType;
using ET.GamePlay;
using ET.Helper;
using ET.Module;
using ET.Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.TestCase
{
    [RobotTestCase(TestCaseId.PvpArena)]
    public class TestCase_PvpArena : IRobotTestCase
    {
        public async ETTask Run()
        {
            for (int i = 0; i < 100; i++)
            {

                DoArenaPvp(i).Coroutine();
            }
        }

        public async ETTask DoArenaPvp(int i)
        {
            Scene clientScene = await RobotHelper.CreateRobot(i + 1);
            await GMHelper.SendGM(clientScene, "UnlockAllModule");
            await GMHelper.SendGM(clientScene, "AddItem", ConstValue.ArenaKeyItemID.ToString(), "1000");
            var bg = clientScene.GetComponent<InventoryComponent>();
            Log.Error("竞技场钥匙数量=" + bg.GetItemCount(ConstValue.ArenaKeyItemID));
            PlayerArenaComponent playerArenaComponen = clientScene.GetComponent<PlayerArenaComponent>();
            playerArenaComponen.arenaBattleReward = new M2C_ArenaBattleReward();
            int battleCount = 0;
            while (true)
            {
                await TimerComponent.Instance.WaitAsync(3000);
                try
                {
                    playerArenaComponen = clientScene.GetComponent<PlayerArenaComponent>();
                    if (playerArenaComponen == null)
                    {
                        Log.Error("PlayerArenaComponent is null");
                        return;
                    }
                    if (playerArenaComponen.arenaBattleReward==null)
                    {
                        Log.Error("等待战斗结束中........");
                        continue;
                    }
                    await playerArenaComponen.GetMyRankInfo();
                    var rse =  await playerArenaComponen.GetRivalList();
                    if (rse.Error == ErrorCode.ERR_Success)
                    {
                        if (rse.rivalList.Count > 0)
                        {

                            var battleres =  await playerArenaComponen.StartArenaBattle(RandomGenerator.RandomNumber(0, rse.rivalList.Count));
                            if (battleres.Error!= ErrorCode.ERR_Success)
                            {
                                Log.Error("开始PVP战斗错误！！！！"+battleres.Error.GetLanguage(LanguageType.CN) );
                                continue;
                            }
                            battleCount++;
                            playerArenaComponen.arenaBattleReward = null;

                            Log.Error($"ArenaPvp  robot{i} battleCount:{battleCount}");
                            var lc = clientScene.CurrentScene().GetComponent<LevelControllerComponent>();
                            if (lc != null)
                            {
                                lc.StopLevel();
                                EventSystem.Instance.Publish(clientScene.CurrentScene(), new OnLevelFinish()
                                {
                                    Success = false,
                                    IgnoreDelay = true
                                });
                            }
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
