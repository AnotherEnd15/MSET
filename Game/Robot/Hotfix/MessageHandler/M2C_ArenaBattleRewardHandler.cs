using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [MessageHandler]
    public class M2C_ArenaBattleRewardHandler_Robot: AMHandler<M2C_ArenaBattleReward>
    {
        protected override void  Run(Session session, M2C_ArenaBattleReward message)
        {
            Log.Error($"M2C_ArenaBattleRewardHandler");
        }
    }
 
}
