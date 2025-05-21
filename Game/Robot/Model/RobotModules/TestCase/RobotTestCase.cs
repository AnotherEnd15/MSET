namespace ET
{
    public class RobotTestCaseAttribute : BaseAttribute
    {
        public int CaseId;

        public RobotTestCaseAttribute(int caseId)
        {
            this.CaseId = caseId;
        }
    }
}