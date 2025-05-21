namespace ET
{
    public class ConditionAttribute : BaseAttribute
    {
        public ConditionType ConditionType;

        public ConditionAttribute(ConditionType statisticType)
        {
            this.ConditionType = statisticType;
        }
    }
}