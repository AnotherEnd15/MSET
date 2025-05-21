namespace ET.ConfigCheck
{
    [ConfigCheck]
    public class TestConfigCheck: IConfigCheck
    {
        public async ETTask Run()
        {
            await ETTask.CompletedTask;
        }
    }
}