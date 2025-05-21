namespace ET.Robot
{
    public static class RobotTestCaseComponentSystem
    {
        public class AwakeSystem : AwakeSystem<RobotTestCaseComponent>
        {
            protected override void Awake(RobotTestCaseComponent self)
            {
                RobotTestCaseComponent.Instance = self;
                self.AllTestCases.Clear();

                foreach (var v in EventSystem.Instance.GetTypes(typeof(RobotTestCaseAttribute)))
                {
                    if(v.IsAbstract || !v.IsClass)
                        continue;

                    var attr =
                        v.GetCustomAttributes(typeof(RobotTestCaseAttribute), false)[0] as RobotTestCaseAttribute;

                    var instance = Activator.CreateInstance(v) as IRobotTestCase;
                    if (instance == null)
                    {
                        Log.GetLogger().Error("实现了RobotTestCaseAttribute 的类没有实现接口 IRobotTestCase {Type}",v.FullName);
                        continue;
                    }

                    self.AllTestCases.Add(attr.CaseId,Activator.CreateInstance(v) as IRobotTestCase);
                }
            }
        }
        
    }
}