namespace ET.Helper
{
    public static class GMHelper
    {
        public static async ETTask SendGM(Scene clientScene, string command, params string[] param)
        {
            var session = clientScene.GateSession();
            var req = new C2M_GMCommandRequest
            {
                Command = command,
            };

            if (param != null)
                req.ParamList.AddRange(param);

            await session.Call(req);
            await TimerComponent.Instance.WaitAsync(300);
        }

    }
}