using ET.Helper;
using ET.Robot;

namespace ET.TestCase
{
    [RobotTestCase(TestCaseId.WorldChat)]
    public class TestCase_WorldChat : IRobotTestCase
    {
        public async ETTask Run()
        {
            Console.WriteLine("开始执行 世界聊天测试用例");
            for (int i = 0; i < 100; i++)
            {
                Scene clientScene = await RobotHelper.CreateRobot(i+1);
                clientScene.GetOrAdd<ChatComponent>();
                await GMHelper.SendGM(clientScene, "UnlockAllModule");
                HandleChat(clientScene).Coroutine();
            }
        }

        async ETTask HandleChat(Scene clientScene)
        {
            while (true)
            {
                await TimerComponent.Instance.WaitAsync(1000);
                try
                {
                    if (clientScene.IsDisposed)
                        return;

                    var req = new C2M_SendChatRequest
                    {
                        ChatInfo = new Proto_ChatInfo(),
                        ChannelType = ChatChannelType.ZoneWorld
                    };
                    
                    if (RandomGenerator.RandomBool())
                    {
                        req.ChatInfo.Content = new ProtoChatContent_Text()
                        {
                            Content = $"{Guid.NewGuid().ToString()}"
                        };
                    }
                    // else
                    // {
                    //     req.ChatInfo.Content = new ProtoChatContent_Emoji()
                    //     {
                    //         EmojiConfigId = ChatEmjiConfigCategory.Instance.DataList.RandomArray().Id
                    //     };
                    // }

                  
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