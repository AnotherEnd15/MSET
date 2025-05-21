namespace ET
{
    public class StateMachineAttribute : BaseAttribute
    {
        public int StateType;

        public StateMachineAttribute(int stateType)
        {
            this.StateType = stateType;
        }
    }
}