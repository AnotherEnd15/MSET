using System.Collections;
using System.Collections.Generic;

namespace ET
{
    public class AttKV
    {
        public NumericType NumericType;
        public long value;

        public AttKV(NumericType numericType, long value)
        {
            NumericType = numericType;
            this.value = value;
        }

    }
}
