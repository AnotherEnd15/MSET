using System;
using System.Collections.Generic;
using FixMath;

namespace ET
{
    namespace EventType
    {
        public struct EntryEvent2
        {
            
        }
        
        public struct FirstLogin
        {
            
        }

        public struct SceneChangeStart1
        {
            public string LastSceneName;
            public LevelConfig LevelConfig;
        }
        
        public struct SceneChangeStart2
        {
            public LevelConfig LevelConfig;
        }

        public struct SceneChangeFinish
        {
        }
        

        public struct AppStartInitFinish
        {
        }

        public struct UnitAdd
        {
        }

        public struct UnitRemove
        {
            
        }

        public struct EnterMapFinish
        {
        }

        public struct LoginFinish
        {
        }

        public struct ReLoginRealm
        {
            
        }

        public struct AfterCreateClientScene
        {
        }

        public struct AfterCreateCurrentScene
        {
        }
    }
}