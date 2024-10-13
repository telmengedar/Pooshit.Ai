namespace Pooshit.Ai.Extensions;

public static class AMath {
    
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
    
    /// <summary>
    /// computes an approximation of the inverse square root of a number
    /// </summary>
    /// <param name="number">number of which to compute inverse square root</param>
    /// <returns>inverse square root</returns>
    public static float InverseSquareRoot(this float number )
    {
        const float threehalfs = 1.5F;

        float x2 = number * 0.5F;
        float y = number;
        int i = BitConverter.SingleToInt32Bits(y);
        i  = 0x5f3759df - ( i >> 1 );
        y  = BitConverter.Int32BitsToSingle(i);
        y *= threehalfs - x2 * y * y;

        return y;
    }
}