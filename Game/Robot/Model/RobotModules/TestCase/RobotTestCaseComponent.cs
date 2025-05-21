namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class RobotTestCaseComponent : Entity,IAwake
    {
        public static RobotTestCaseComponent Instance;
        
        public Dictionary<int, IRobotTestCase> AllTestCases = new();
    }
}