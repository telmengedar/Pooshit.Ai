using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net;

/// <summary>
/// math operations used for neuronal values
/// </summary>
public static class NMath {

    /// <summary>
    /// computes values in a neuronal net
    /// </summary>
    /// <param name="lhs">lhs neuron value</param>
    /// <param name="rhs">rhs neuron value or connection weight</param>
    /// <param name="op">operation to apply</param>
    /// <returns>result</returns>
    public static float Compute(float lhs, float rhs, OperationType op) {
        float result;
        switch (op) {
            default:
            case OperationType.Multiply:
                result = lhs * rhs;
                break;
            case OperationType.Add:
                result = lhs + rhs;
                break;
            case OperationType.Pow:
                result = (float)AMath.Power(lhs, Math.Abs(rhs));
                break;
            case OperationType.InvPow:
                result = (float)AMath.Power(rhs, Math.Abs(lhs));
                break;
            case OperationType.Div:
                result = lhs / rhs;
                if (float.IsNaN(result) || float.IsInfinity(result) || float.IsNegativeInfinity(result))
                    result = 0.0f;
                break;
            case OperationType.Sub:
                result = lhs - rhs;
                break;
        }

        return result;
    }

    public static float Activation(this float value, ActivationFunc func) {
        float result;
        switch (func) {
            default:
            case ActivationFunc.None:
                result=value;
                break;
            case ActivationFunc.BinaryStep:
                result= value < 0.0f ? 0.0f : 1.0f;
                break;
            case ActivationFunc.Sigmoid:
                result= 0.5f * (value / (1.0f + Math.Abs(value))) + 0.5f;
                break;
            case ActivationFunc.Sin:
                result= (float)Math.Sin(value);
                break;
            case ActivationFunc.Tanh:
                result= (float)Math.Tanh(value);
                break;
            case ActivationFunc.ReLU:
                result= Math.Max(0.0f, value);
                break;
            case ActivationFunc.LeakyReLU:
                result= Math.Max(0.1f * value, value);
                break;
            case ActivationFunc.Reciprocal:
                result= 1.0f / value;
                break;
            case ActivationFunc.Swish:
                result= value * (0.5f * (value / (1.0f + Math.Abs(value))) + 0.5f);
                break;
            case ActivationFunc.Pow2:
                result= value * value;
                break;
            case ActivationFunc.Sqrt:
                result= (float)AMath.Power(value, 0.5);
                break;
        }

        if (float.IsNaN(result) || float.IsInfinity(result) || float.IsNegativeInfinity(result))
            return 0.0f;
        return result;
    }
    
    /// <summary>
    /// set value of neuron
    /// </summary>
    /// <param name="input">input values</param>
    /// <param name="aggregate">aggregate func</param>
    public static float Aggregate(this IEnumerable<float> input, AggregateType aggregate) {
        float[] values = input.ToArray();
        if (values.Length == 0)
            return 0.0f;
        
        switch (aggregate) {
            default:
            case AggregateType.Sum:
                return values.Sum();
            case AggregateType.Average:
                return values.Average();
            case AggregateType.Median:
                Array.Sort(values);
                int middle = values.Length >> 1;
                return values[middle];
            case AggregateType.Min:
                return values.Min();
            case AggregateType.Max:
                return values.Max();
        }
    }

    /// <summary>
    /// generates an int64 hash from a series of hashes
    /// </summary>
    /// <param name="hashes">hashes used to generate int64 hash</param>
    /// <returns>int64 hash</returns>
    public static long GenerateHash(this IEnumerable<int> hashes) {
        ulong value = 0;
        foreach (int hash in hashes) {
            value = (value << 5) | (value >> 59);
            value ^= (ulong)hash;
        }

        return (long)value;
    }
}