using System;

namespace ET
{
    [FriendOf(typeof(NetThreadComponent))]
    public static class NetThreadComponentSystem
    {
        [ObjectSystem]
        public static void Awake(this NetThreadComponent self)
        {
            NetThreadComponent.Instance = self;

            // // 网络线程
            // self.thread = new Thread(self.NetThreadUpdate);
            // self.thread.Start();
        }
        
        [ObjectSystem]
        public static void LateUpdate(this NetThreadComponent self)
        {
            self.MainThreadUpdate();
            NetServices.Instance.UpdateInNetThread();
        }
        
        [ObjectSystem]
        public static void Destroy(this NetThreadComponent self)
        {
            NetThreadComponent.Instance = null;
            self.isStop = true;
            // self.thread.Join(1000);
        }

        // 主线程Update
        private static void MainThreadUpdate(this NetThreadComponent self)
        {
            NetServices.Instance.UpdateInMainThread();
        }

        // // 网络线程Update
        // private static void NetThreadUpdate(this NetThreadComponent self)
        // {
        //     while (!self.isStop)
        //     {
        //         NetServices.Instance.UpdateInNetThread();
        //         Thread.Sleep(1);
        //     }
        // }
    }
}