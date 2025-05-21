using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    namespace EventType
    {
        public struct NumbericChange
        {
            public NumericType NumericType;
            public long Old;
            public long New;
        }
    }
    
    public static class NumericComponentSys
    {

        public static NumericType GetBaseNumeric(this NumericType origin)
        {
            if ((int)origin > NumericComponent.NumericMax)
            {
                return origin;
            }

            var config = NumericConfigCategory.Instance.Get((int)origin);
            if (config.GenAdjust == 0)
            {
                return origin;
            }
            
            return (NumericType)((int)origin * 10 + 1);
        }

        public static int GetAsInt(this NumericComponent self, NumericType numericType)
        {
            return (int)self.GetByKey(numericType);
        }

        public static long GetAsLong(this NumericComponent self, NumericType numericType)
        {
            return self.GetByKey(numericType);
        }
        public static float GetAsFloat(this NumericComponent self, NumericType numericType)
        {
            return self.GetByKey(numericType)*0.0001f;
        }

        public static void Set(this NumericComponent self, NumericType nt, float value)
        {
            self[nt] = (long)(value * 10000);
        }

        public static void Set(this NumericComponent self, NumericType nt, int value)
        {
            self[nt] = value;
        }

        public static void Set(this NumericComponent self, NumericType nt, long value)
        {
            self[nt] = value;
        }

        public static void SetNoEvent(this NumericComponent self, NumericType numericType, long value)
        {
            self.Insert(numericType, value, false);
        }

        public static void ChangeNoEvent(this NumericComponent self, NumericType numericType, long value)
        {
            var oldValue = self[numericType];
            oldValue = oldValue + value;
            self.Insert(numericType,oldValue,false);
        }
        
        public static void Change(this NumericComponent self, NumericType numericType, long value)
        {
            var oldValue = self[numericType];
            oldValue = oldValue + value;
            self[numericType] = oldValue;
        }

        public static void Insert(this NumericComponent self, NumericType numericType, long value, bool isPublicEvent = true)
        {
            long oldValue = self.GetByKey(numericType);
            if (oldValue == value)
            {
                return;
            }

            self.NumericDic[numericType] = value;

            if ((int)numericType >= NumericComponent.NumericMax)
            {
                self.Update(numericType, isPublicEvent);
                return;
            }

            if (isPublicEvent && !self.InInit)
            {
                EventSystem.Instance.Publish(self.GetParent<Unit>(),
                    new EventType.NumbericChange() { New = value, Old = oldValue, NumericType = numericType });
            }
        }

        public static long GetByKey(this NumericComponent self, NumericType key)
        {
            long value = 0;
            self.NumericDic.TryGetValue(key, out value);
            return value;
        }

        public static void Update(this NumericComponent self, NumericType numericType, bool isPublicEvent)
        {
            int final = (int)numericType / 10;
            NumericType bas = (NumericType)(final * 10 + 1);
            NumericType add = (NumericType)(final * 10 + 2);
            NumericType pct = (NumericType)(final * 10 + 3);
            

            
            var fixedNum = self.GetByKey(bas) + self.GetByKey(add);
            
            var result = fixedNum * (10000 + self.GetAsInt(pct)) / 10000;
            self.Insert((NumericType)final, result, isPublicEvent);
        }
        
    }

    [ComponentOf(typeof (Unit))]
    public class NumericComponent: Entity, IAwake
    {
        public const int NumericMax = 10000;
        
        public Dictionary<NumericType, long> NumericDic = new Dictionary<NumericType, long>();
        
        public bool InInit;
        
        public long this[NumericType numericType]
        {
            get
            {
                return this.GetByKey(numericType);
            }
            
            set
            {
                this.Insert(numericType, value);
            }
        }
    }
}