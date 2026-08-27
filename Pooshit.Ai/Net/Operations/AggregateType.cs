namespace Pooshit.Ai.Net.Operations;

/// <summary>
/// type of aggregate to apply to target neurons
/// </summary>
public enum AggregateType {
    
    /// <summary>
    /// sum of all values
    /// </summary>
    Sum,

    /// <summary>
    /// arithmetic mean of all values
    /// </summary>
    Average,

    /// <summary>
    /// middle value of the sorted values, or the upper of the two middle values for an even count — not the mean of the two middle values
    /// </summary>
    Median,

    /// <summary>
    /// smallest of all values
    /// </summary>
    Min,

    /// <summary>
    /// largest of all values
    /// </summary>
    Max,

    /// <summary>
    /// 0.9 times the average of all values plus 0.1 times the maximum value
    /// </summary>
    AverageToMax
}