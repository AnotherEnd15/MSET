using System.Collections.Generic;

namespace ET
{
    namespace EventType
    {
        public struct CurrencyChange
        {
            public CurrencyType CurrencyType;
            public long Value;
        }
        
        public struct CloseServer
        {
            public long CloseTime;
        }
        
        public struct RepeatLogin
        {
            
        }

        public struct OnDisconnect
        {
            public int error { get; set; }
        }

        public struct ReconnectStart
        {
            
        }

        public struct ReconnectEnd
        {
            
        }
        
    }
}