namespace Pooshit.Ai.Extensions;

public static class AMath {
    
    /// <summary>
    /// fast approximation of power
    /// </summary>
    /// <param name="a">base</param>
    /// <param name="b">exponent</param>
    /// <returns>approximation of a to the power of b, or NaN if a is less than or equal to 0</returns>
    public static double Power(double a, double b) {
        if (a <= 0.0)
            return double.NaN;

        int tmp = (int)(BitConverter.DoubleToInt64Bits(a) >> 32);
        int tmp2 = (int)(b * (tmp - 1072632447) + 1072632447);
        return BitConverter.Int64BitsToDouble(((long)tmp2) << 32);
    }

    /// <summary>
    /// computes an approximation of the inverse square root of a number
    /// </summary>
    /// <param name="number">number of which to compute inverse square root</param>
    /// <returns>approximate inverse square root, or NaN if number is negative</returns>
    public static float InverseSquareRoot(this float number )
    {
        if (number < 0.0f)
            return float.NaN;

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