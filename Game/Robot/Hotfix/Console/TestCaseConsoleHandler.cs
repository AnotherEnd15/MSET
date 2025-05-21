namespace ET
{
    [ConsoleHandler(ConsoleMode.TestCase)]
    public class TestCaseConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContex contex, string content)
        {
            contex.Parent.RemoveComponent<ModeContex>();

            var strs = content.Split(" ");

            if (strs == null || strs.Length != 2)
            {
                Console.WriteLine($"参数不符合 {content} 预期参数数量1");
                return;
            }

            content = strs[1];

            if (!int.TryParse(content, out var caseId))
            {
                Console.WriteLine($"TestCase参数异常 无法将{content} 转换为有效int值");
                return;
            }

            if (!RobotTestCaseComponent.Instance.AllTestCases.TryGetValue(caseId, out var handler))
            {
                Console.WriteLine($"不存在{caseId}对应的测试用例");
                return;
            }
                    
            handler.Run().Coroutine();
            
            await ETTask.CompletedTask;
        }
    }
}