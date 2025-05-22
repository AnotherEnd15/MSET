using System.Collections.Generic;
using ET.Proto;
using MongoDB.Bson;

namespace ET
{
    public static class OpcodeHelper
    {
        [StaticField]
        private static readonly HashSet<ushort> ignoreDebugLogMessageSet = new HashSet<ushort>
        {
            OuterMessage.C2G_Ping,
            OuterMessage.G2C_Ping,
            ushort.MaxValue, // ActorResponse
        };


        // 协议cd 除了特殊的 一般只有有资源消耗的协议才允许短cd间隔发送
        [StaticField] public static Dictionary<ushort, long> ProtoCDDefine = new()
        {
           
        };

        private static bool IsNeedLogMessage(ushort opcode)
        {
            if (ignoreDebugLogMessageSet.Contains(opcode))
            {
                return false;
            }

            return true;
        }

        public static bool IsOuterMessage(ushort opcode)
        {
            return opcode < OpcodeRangeDefine.OuterMaxOpcode;
        }

        public static bool IsInnerMessage(ushort opcode)
        {
            return opcode >= OpcodeRangeDefine.OuterMaxOpcode && opcode <= OpcodeRangeDefine.InnerMaxOpcode;
        }
        
        public static void LogMsg(int zone, object message)
        {
#if UNITY_EDITOR
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            if (!IsNeedLogMessage(opcode))
            {
                return;
            }

            Log.Debug($"zone: {zone} {message.GetType().FullName} {JsonHelper.ToJson(message)}");
#endif
        }

        public static void LogMsg(long actorId, object message)
        {
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            if (!IsNeedLogMessage(opcode))
            {
                return;
            }
            
            Log.GetLogger().Debug($"actorId: {actorId} {message}");
        }
        
    }
}