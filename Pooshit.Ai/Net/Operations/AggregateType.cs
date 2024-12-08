namespace Pooshit.Ai.Net.Operations;

/// <summary>
/// type of aggregate to apply to target neurons
/// </summary>
public enum AggregateType {
    
    /// <summary>
    /// adds all values
    /// </summary>
    Sum,
    
    /// <summary>
    /// generate the average of all values
    /// </summary>
    Average,
    
    /// <summary>
    /// generates the median of all values
    /// </summary>
    Median,
    
    /// <summary>
    /// generates the minimum of all values
    /// </summary>
    Min,
    
    /// <summary>
    /// generates the maximum of all values
    /// </summary>
    Max,
    
    /// <summary>
    /// average of average and max
    /// </summary>
    AverageToMax
}