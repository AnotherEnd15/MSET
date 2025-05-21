using System;
using System.Collections.Generic;

namespace ET
{
    public static class FrameTimerComponentSystem
    {
        static long GetId(this FrameTimerComponent self)
        {
            return ++self.idGenerator;
        }

        public static long GetNow(this FrameTimerComponent self)
        {
            return self.CurrFrame;
        }

        public static void ResetTimerComponent(this Scene scene)
        {
            var oldTimerCom = scene.GetComponent<FrameTimerComponent>();
            using var set = HashSetComponent<TimerAction>.Create();
            // 保留所有repeated timer
            foreach (var v in oldTimerCom.timerActions)
            {
                if (v.Value.TimerClass == TimerClass.RepeatedTimer)
                {
                    set.Add(v.Value);
                }
            }
            
            scene.RemoveComponent<FrameTimerComponent>();
            var newTimerCom = scene.AddComponent<FrameTimerComponent>();
            var now = newTimerCom.GetNow();
            foreach (var v in set)
            {
                v.StartTime = now;
                newTimerCom.AddTimer(v);
            }
        }

        public static void Update(this FrameTimerComponent self,long currFrame)
        {
            self.CurrFrame = currFrame;
            
            if (self.TimeId.Count == 0)
            {
                return;
            }

            long timeNow = self.CurrFrame ;

            if (timeNow < self.minTime)
            {
                return;
            }

            foreach (KeyValuePair<long, List<long>> kv in self.TimeId)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    self.minTime = k;
                    break;
                }

                self.timeOutTime.Enqueue(k);
            }

            while (self.timeOutTime.Count > 0)
            {
                long time = self.timeOutTime.Dequeue();
                var list = self.TimeId[time];
                for (int i = 0; i < list.Count; ++i)
                {
                    long timerId = list[i];
                    self.timeOutTimerIds.Enqueue(timerId);
                }
                self.TimeId.Remove(time);
            }

            while (self.timeOutTimerIds.Count > 0)
            {
                long timerId = self.timeOutTimerIds.Dequeue();

                if (!self.timerActions.Remove(timerId, out TimerAction timerAction))
                {
                    continue;
                }
                
                self.Run(timerAction);
            }
        }

        static void Run(this FrameTimerComponent self, TimerAction timerAction)
        {
            switch (timerAction.TimerClass)
            {
                case TimerClass.OnceTimer:
                {
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    timerAction.Recycle();
                    break;
                }
                case TimerClass.OnceWaitTimer:
                {
                    ETTask tcs = timerAction.Object as ETTask;
                    tcs.SetResult();
                    timerAction.Recycle();
                    break;
                }
                case TimerClass.RepeatedTimer:
                {                    
                    self.AddTimer(timerAction);
                    long timeNow = self.GetNow();
                    while (timeNow-timerAction.StartTime>=timerAction.Time)
                    {
                        timerAction.StartTime += timerAction.Time;
                        EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                        if (timerAction.Id == 0)
                            return;
                    }
                    break;
                }
            }
        }

        static void AddTimer(this FrameTimerComponent self,TimerAction timer)
        {
            if (self.IsDisposed)
            {
                Log.GetLogger().Error($"FrameTimerComponent已经释放 仍然有添加Timer的请求 {timer.TimerClass} {timer.Type}");
                return;
            }

            long tillTime = timer.StartTime + timer.Time;
            self.TimeId.Add(tillTime, timer.Id);
            self.timerActions.Add(timer.Id, timer);
            if (tillTime < self.minTime)
            {
                self.minTime = tillTime;
            }
        }

        public static bool Remove(this FrameTimerComponent self,ref long id)
        {
            long i = id;
            id = 0;
            return self.Remove(i);
        }

        static bool Remove(this FrameTimerComponent self,long id)
        {
            if (id == 0)
            {
                return false;
            }

            if (!self.timerActions.Remove(id, out TimerAction timerAction))
            {
                return false;
            }
            timerAction.Recycle();
            return true;
        }
        
        public static async ETTask WaitFrameAsync(this FrameTimerComponent self,long frame, ETCancellationToken cancellationToken = null)
        {
            if (frame == 0)
            {
                return;
            }

            long timeNow = self.GetNow();

            ETTask tcs = ETTask.Create(true);
            TimerAction timer = TimerAction.Create(self.GetId(), TimerClass.OnceWaitTimer, timeNow, frame, 0, tcs);
            self.AddTimer(timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (self.Remove(timerId))
                {
                    tcs.SetResult();
                }
            }

            try
            {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        }

        // 用这个优点是可以热更，缺点是回调式的写法，逻辑不连贯。WaitTillAsync不能热更，优点是逻辑连贯。
        // wait时间短并且逻辑需要连贯的建议WaitTillAsync
        // wait时间长不需要逻辑连贯的建议用NewOnceTimer
        public static long NewOnceFrameTimer(this FrameTimerComponent self,long tillTime, int type, object args)
        {
            long timeNow = self.GetNow();
            if (tillTime < timeNow)
            {
                throw new Exception("till time< now!");
            }

            TimerAction timer = TimerAction.Create(self.GetId(), TimerClass.OnceTimer, timeNow, tillTime - timeNow, type, args);
            self.AddTimer(timer);
            return timer.Id;
        }
        
        /// <summary>
        /// 创建一个RepeatedTimer
        /// </summary>
        private static long NewRepeatedFrameTimerInner(this FrameTimerComponent self,long frame, int type, object args)
        {
#if DOTNET
            if (frame < 1)
            {
                throw new Exception($"repeated timer < 1, timerType: time: {frame}");
            }
#endif
            
            long timeNow = self.GetNow();
            TimerAction timer = TimerAction.Create(self.GetId(), TimerClass.RepeatedTimer, timeNow, frame, type, args);

            // 每帧执行的不用加到timerId中，防止遍历
            self.AddTimer(timer);
            return timer.Id;
        }

        public static long NewRepeatedFrameTimer(this FrameTimerComponent self,long frame, int type, object args)
        {
            return self.NewRepeatedFrameTimerInner(frame, type, args);
        }
    }
}