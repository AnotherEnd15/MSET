﻿namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class BenchmarkServerComponent: Entity, IAwake
    {
        public int Count { get; set; }
    }
}