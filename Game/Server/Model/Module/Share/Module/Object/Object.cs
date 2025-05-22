using System;

namespace ET
{
    [Serializable]
    public abstract class Object
    {
        public override string ToString()
        {
#if DOTNET
            return MongoHelper.ToJson(this);
#else
            return $"type={this.GetType().FullName} ";
#endif
        }
    }
}