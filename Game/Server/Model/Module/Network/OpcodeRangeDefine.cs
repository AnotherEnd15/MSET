namespace ET
{
    public static class OpcodeRangeDefine
    {
        public const ushort MinOpcode = 1;
        public const ushort OuterMaxOpcode = 10000;

        // 10001-20000 内网
        public const ushort InnerMaxOpcode = 20000;
        
        public const ushort MaxOpcode = 60000;
    }
}