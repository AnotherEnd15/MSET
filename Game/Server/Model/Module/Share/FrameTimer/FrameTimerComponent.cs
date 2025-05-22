using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class FrameTimerComponent : Entity,IAwake,IDestroy
    {
        /// <summary>
        /// key: time, value: timer id
        /// </summary>
        public readonly MultiMap<long, long> TimeId = new();

        public readonly Queue<long> timeOutTime = new();

        public readonly Queue<long> timeOutTimerIds = new();

        public readonly Dictionary<long, TimerAction> timerActions = new();

        public long idGenerator;

        // 记录最小时间，不用每次都去MultiMap取第一个值
        public long minTime = long.MaxValue;

        public long CurrFrame;
    }
}