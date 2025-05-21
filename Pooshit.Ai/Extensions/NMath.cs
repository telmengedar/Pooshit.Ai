using Pooshit.Ai.Net.Operations;

namespace Pooshit.Ai.Extensions;

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
            case OperationType.Max:
                result=Math.Max(lhs, rhs);
            break;
            case OperationType.Min:
                result = Math.Min(lhs, rhs);
            break;
        }

        return result;
    }

    /// <summary>
    /// executes the specified activation function
    /// </summary>
    /// <param name="value">value to be used as argument for function</param>
    /// <param name="func">function to execute</param>
    /// <returns>function result</returns>
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
                result= MathF.Sin(value);
                break;
            case ActivationFunc.Tanh:
                result= MathF.Tanh(value);
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
                result = 1.0f / value.InverseSquareRoot();
                break;
            case ActivationFunc.Floor:
                result = MathF.Floor(value);
                break;
            case ActivationFunc.Ceiling:
                result = MathF.Ceiling(value);
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
        switch (aggregate) {
            default:
            case AggregateType.Sum:
                return input.Sum();
            case AggregateType.Average:
                return input.Average();
            case AggregateType.Median:
                float[] values = input.ToArray();
                Array.Sort(values);
                int middle = values.Length >> 1;
                return values[middle];
            case AggregateType.Min:
                return input.Min();
            case AggregateType.Max:
                return input.Max();
            case AggregateType.AverageToMax:
                return AverageToMax(input);
        }
    }

    /// <summary>
    /// function used to compute fitness value
    /// </summary>
    /// <param name="values">deviation values</param>
    /// <returns>fitness value</returns>
    public static float AverageToMax(this IEnumerable<float> values) {
        float max = 0.0f;
        float sum = 0.0f;
        int count = 0;
        foreach (float value in values) {
            max = MathF.Max(max, value);
            sum += value;
            ++count;
        }

        if (count == 0)
            return 0.0f;

        return (sum / count + max) * 0.5f;
    }
}