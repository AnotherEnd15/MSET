using System;
using System.IO;
using System.Threading.Tasks;
using ET.Define;
using ET.Server;
using Luban;
using MongoDB.Driver;

namespace ET
{
    [ConsoleHandler(ConsoleMode.ClearDB)]
    public class ClearDBConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContex contex, string content)
        {
            if (Options.Instance.Develop == 0)
                return;
            
            foreach (var db in DBManagerComponent.Instance.DBComponents.Values)
            {
                db.mongoClient.DropDatabase(db.DBName);
            }
            
            Console.WriteLine("清理DB完成.请重新起服.2秒后自动关闭");
            Environment.Exit(0);

            await ETTask.CompletedTask;
        }
    }
}