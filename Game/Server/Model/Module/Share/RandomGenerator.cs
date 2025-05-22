using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace ET
{
    // 为了保证客户端逻辑层不小心使用 把这个放到显示层
    public static class RandomGenerator
    {
        [StaticField]
        [ThreadStatic]
        private static Random random;

        private static Random GetRandom()
        {
            return random ??= new Random(Guid.NewGuid().GetHashCode());
        }

        public static ulong RandUInt64()
        {
            int r1 = RandInt32();
            int r2 = RandInt32();
            
            return ((ulong)r1 << 32) & (ulong)r2;
        }

        public static int RandInt32()
        {
            return GetRandom().Next();
        }

        public static uint RandUInt32()
        {
            return (uint) GetRandom().Next();
        }

        public static long RandInt64()
        {
            uint r1 = RandUInt32();
            uint r2 = RandUInt32();
            return (long)(((ulong)r1 << 32) | r2);
        }

        /// <summary>
        /// 获取lower与Upper之间的随机数,包含下限，不包含上限
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int RandomNumber(int lower, int upper)
        {
            int value = GetRandom().Next(lower, upper);
            return value;
        }

        public static bool RandomBool()
        {
            return GetRandom().Next(2) == 0;
        }

        public static T RandomArray<T>(T[] array)
        {
            return array[RandomNumber(0, array.Length)];
        }

        public static T RandomArray<T>(this List<T> array)
        {
            return array[RandomNumber(0, array.Count)];
        }

        public static T RandomArrayByWeight<T>(this List<T> array, List<int> weightList)
        {
            if (array.Count != weightList.Count)
            {
                throw new Exception("按权重随机,给出的权重数量不符");
            }

            int totalWeight = 0;
            weightList.ForEach(v => totalWeight += v);
            var randomWeight = RandomGenerator.RandomNumber(0, totalWeight);

            int currWeight = 0;
            for (int i = 0; i < weightList.Count; i++)
            {
                currWeight += weightList[i];
                if (randomWeight < currWeight)
                {
                    return array[i];
                }
            }

            return array.First();
        }
        
        public static T RandomArrayByWeight<T>(this T[] array, int[] weightList)
        {
            if (array.Length != weightList.Length)
            {
                throw new Exception("按权重随机,给出的权重数量不符");
            }

            int totalWeight = 0;
            Array.ForEach(weightList, v => totalWeight += v);
            var randomWeight = RandomGenerator.RandomNumber(0, totalWeight);

            int currWeight = 0;
            for (int i = 0; i < weightList.Length; i++)
            {
                currWeight += weightList[i];
                if (randomWeight < currWeight)
                {
                    return array[i];
                }
            }

            return array.First();
        }
        
        public static T RandomArrayByWeight<T>(this List<T> array, Func<T,int> getWeight)
        {
            
            int totalWeight = 0;
            array.ForEach(v => totalWeight += getWeight.Invoke(v));
            var randomWeight = RandomGenerator.RandomNumber(0, totalWeight);

            int currWeight = 0;
            for (int i = 0; i < array.Count; i++)
            {
                currWeight += getWeight.Invoke(array[i]);
                if (randomWeight < currWeight)
                {
                    return array[i];
                }
            }

            return array.First();
        }

        /// <summary>
        /// 打乱数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">要打乱的数组</param>
        public static void BreakRank<T>(List<T> arr)
        {
            if (arr == null || arr.Count < 2)
            {
                return;
            }

            for (int i = 0; i < arr.Count; i++)
            {
                int index = GetRandom().Next(0, arr.Count);
                (arr[index], arr[i]) = (arr[i], arr[index]);
            }
        }

        public static float RandFloat01()
        {
            int a = RandomNumber(0, 100);
            return a / 100f;
        }
    }
}