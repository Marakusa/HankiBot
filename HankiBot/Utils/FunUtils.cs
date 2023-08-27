namespace HankiBot.Utils
{
    public static class FunUtils
    {
        public static int CalculateRandom(int offset, ulong userId, int dayOfYear, int maxValue = 100)
        {
            int seed = userId.GetHashCode() + dayOfYear + offset;
            return GetRandom(seed, maxValue);
        }

        public static int CalculateRandom(int offset, ulong userId, ulong userIdAnother, int dayOfYear, int maxValue = 100)
        {
            int seed = userId.GetHashCode() + userIdAnother.GetHashCode() + dayOfYear + offset;
            return GetRandom(seed, maxValue);
        }

        public static T PickRandom<T>(T[] list)
        {
            DateTime now = DateTime.Now;
            return list[GetRandom(now.Millisecond + now.Second + now.Minute + now.Hour, list.Length - 1)];
        }

        private static int GetRandom(int seed, int maxValue)
        {
            Random random = new(seed);
            return random.Next(0, maxValue + 1);
        }
    }
}
