namespace NightlyCode.Ai.Extensions;

static class AMath {
    
    /// <summary>
    /// fast approximation of power
    /// </summary>
    /// <param name="a">base</param>
    /// <param name="b">exponent</param>
    /// <returns>approximation of a to the power of b</returns>
    public static double Power(double a, double b) {
        int tmp = (int)(BitConverter.DoubleToInt64Bits(a) >> 32);
        int tmp2 = (int)(b * (tmp - 1072632447) + 1072632447);
        return BitConverter.Int64BitsToDouble(((long)tmp2) << 32);
    }
}