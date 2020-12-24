namespace Utility
{
    public static class Math
    {
        /// <summary>
        /// Calculate a number which is the minimum among the power of two numbers more than <paramref name="n"/>.
        /// </summary>
        /// <param name="n">Number of the minimum</param>
        /// <returns>Number which is the minimum among the power of two numbers more than <paramref name="n"/></returns>
        public static uint NextPow2(int n)
        {
            // nが0以下の時は0とする。
            if (n <= 0) return 0;

            // (n & (n - 1)) == 0 の時は、nが2の冪乗であるため、そのままnを返す。
            if ((n & (n - 1)) == 0) return (uint)n;

            // bitシフトを用いて、2の冪乗を求める。
            uint ret = 1;
            while (n > 0) { ret <<= 1; n >>= 1; }
            return ret;
        }
    }
}
