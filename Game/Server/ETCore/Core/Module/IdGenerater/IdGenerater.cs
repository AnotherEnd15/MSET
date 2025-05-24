using System;
using System.Runtime.InteropServices;

namespace ET
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IdStruct
    {
        public uint Time;    // 32bit  秒数 126+年
        public int Process;  // 16bit
        public ushort Value; // 16bit

        public long ToLong()
        {
            ulong result = 0;
            result |= this.Value;
            result |= (ulong) this.Process << 16;
            result |= (ulong) this.Time << 32;
            return (long) result;
        }

        public IdStruct(uint time, int process, ushort value)
        {
            this.Process = process;
            this.Time = time;
            this.Value = value;
        }

        public IdStruct(long id)
        {
            ulong result = (ulong) id; 
            this.Value = (ushort) (result & ushort.MaxValue);
            result >>= 16;
            this.Process = (int) (result & IdGenerater.Mask16bit);
            result >>= 16;
            this.Time = (uint) result;
        }

        public override string ToString()
        {
            return $"process: {this.Process}, time: {this.Time}, value: {this.Value}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InstanceIdStruct
    {
        public uint Time;   // 当年开始的tick 28bit
        public int Process; // 16bit
        public uint Value;  // 20bit

        public long ToLong()
        {
            ulong result = 0;
            result |= this.Value;
            result |= (ulong)this.Process << 20;
            result |= (ulong) this.Time << 36;
            return (long) result;
        }

        public InstanceIdStruct(long id)
        {
            ulong result = (ulong) id;
            this.Value = (uint)(result & IdGenerater.Mask20bit);
            result >>= 20;
            this.Process = (int)(result & IdGenerater.Mask16bit);
            result >>= 16;
            this.Time = (uint)result;
        }

        public InstanceIdStruct(uint time, int process, uint value)
        {
            this.Time = time;
            this.Process = process;
            this.Value = value;
        }
        
        // 给SceneId使用
        public InstanceIdStruct(int process, uint value)
        {
            this.Time = 0;
            this.Process = process;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"process: {this.Process}, value: {this.Value} time: {this.Time}";
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SceneIdStruct
    {
        public uint Zone; // 24bit
        public uint SceneIndex; // 8bit

        public uint ToUInt()
        {
            uint result = 0;
            result |= this.SceneIndex;
            result |= this.Zone << 8;
            return result;
        }

        public SceneIdStruct(uint value)
        {
            this.SceneIndex = (uint)(value & 0xff);
            value >>= 18;
            this.Zone = value;
        }

        public SceneIdStruct(uint zone,uint sceneIndex)
        {
            this.Zone = zone;
            this.SceneIndex = sceneIndex;
        }

        public override string ToString()
        {
            return $"zone: {this.Zone} sceneIndex: {this.SceneIndex}";
        }
    }

    public class IdGenerater: Singleton<IdGenerater>
    {
        public const int Mask20bit = 0x0fffff;
        public const int Mask16bit = ushort.MaxValue;

        private long epoch2020;
        private ushort value;
        private uint lastIdTime;

        
        private long epochThisYear;
        private uint instanceIdValue;
        private uint lastInstanceIdTime;

        public IdGenerater()
        {
            long epoch1970tick = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
            this.epoch2020 = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970tick;
            this.epochThisYear = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970tick;
            
            this.lastInstanceIdTime = TimeSinceThisYear();
            if (this.lastInstanceIdTime <= 0)
            {
                Log.GetLogger().Warning($"lastInstanceIdTime less than 0: {this.lastInstanceIdTime}");
                this.lastInstanceIdTime = 1;
            }
            this.lastIdTime = TimeSince2020();
            if (this.lastIdTime <= 0)
            {
                Log.GetLogger().Warning($"lastIdTime less than 0: {this.lastIdTime}");
                this.lastIdTime = 1;
            }
        }

        private uint TimeSince2020()
        {
            uint a = (uint)((TimeInfo.Instance.FrameTime - this.epoch2020) / 1000);
            return a;
        }
        
        private uint TimeSinceThisYear()
        {
            uint a = (uint)((TimeInfo.Instance.FrameTime - this.epochThisYear) / 1000);
            return a;
        }
        
        public long GenerateInstanceId(int process)
        {
            uint time = TimeSinceThisYear();

            if (time > this.lastInstanceIdTime)
            {
                this.lastInstanceIdTime = time;
                this.instanceIdValue = 0;
            }
            else
            {
                ++this.instanceIdValue;
                
                if (this.instanceIdValue > Mask20bit - 1) // 20bit
                {
                    ++this.lastInstanceIdTime; // 借用下一秒
                    this.instanceIdValue = 0;

                    Log.GetLogger().Error($"instanceid count per sec overflow: {time} {this.lastInstanceIdTime}");
                }
            }

            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(this.lastInstanceIdTime, process, this.instanceIdValue);
            return instanceIdStruct.ToLong();
        }

        public long GenerateId(int process)
        {
            uint time = TimeSince2020();

            if (time > this.lastIdTime)
            {
                this.lastIdTime = time;
                this.value = 0;
            }
            else
            {
                ++this.value;
                
                if (value > ushort.MaxValue - 1)
                {
                    this.value = 0;
                    ++this.lastIdTime; // 借用下一秒
                    Log.GetLogger().Error($"id count per sec overflow: {time} {this.lastIdTime}");
                }
            }
            
            IdStruct idStruct = new IdStruct(this.lastIdTime, process, value);
            return idStruct.ToLong();
        }
    }
}