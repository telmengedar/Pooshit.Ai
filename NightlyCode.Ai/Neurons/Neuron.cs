using System.Globalization;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Neurons;

/// <summary>
/// a neuron in a neuronal net
/// </summary>
public class Neuron {
    
    /// <summary>
    /// value of neuron
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// set value of neuron
    /// </summary>
    /// <param name="input"></param>
    /// <param name="aggregate"></param>
    public bool SetValue(IEnumerable<float> input, AggregateType aggregate) {
        float[] values = input.ToArray();
        if (values.Length == 0) {
            Value = 0.0f;
            return true;
        }
        
        switch (aggregate) {
            default:
            case AggregateType.Sum:
                float sum = values.Sum();
                if (!double.IsNaN(sum) && !double.IsInfinity(sum) && !double.IsNegativeInfinity(sum))
                    Value = sum;
                else {
                    Value = 0.0f;
                    return true;
                }
                break;
            case AggregateType.Average:
                Value = values.Average();
                break;
            case AggregateType.Median:
                Array.Sort(values);
                int middle = values.Length / 2;
                Value = values[middle];
                break;
            case AggregateType.Min:
                Value = values.Min();
                break;
            case AggregateType.Max:
                Value = values.Max();
                break;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}