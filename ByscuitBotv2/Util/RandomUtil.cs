using System;

namespace ByscuitBotv2.Util
{
    public static class RandomUtil
    {
        
        public static Random Random = new Random();

        public static uint RandomUInt32()
        {
            var array = new byte[4];
            Random.NextBytes(array);
            
            return BitConverter.ToUInt32(array, 0);
        }
        
    }
}
