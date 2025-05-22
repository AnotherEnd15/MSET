using System;

namespace ET
{
    public interface IStateMachine
    {
        void Enter(object obj);
        void Exit(object obj);

        void Update(object obj,int deltaFrame);

        Type GetSystemType();
    }

    public abstract class AStateMachine<T>: IStateMachine where T : class
    {
        public void Enter(object obj)
        {
            InternalEnter(obj as T);
        }

        public void Exit(object obj)
        {
            InternalExit(obj as T);
        }

        public void Update(object obj,int deltaFrame)
        {
            InternalUpdate(obj as T, deltaFrame);
        }
        
        protected abstract void InternalEnter(T self);

        protected abstract void InternalUpdate(T self,int deltaFrame);
        
        protected abstract void InternalExit(T self);

        public Type GetSystemType()
        {
            return typeof (T);
        }
    }
}