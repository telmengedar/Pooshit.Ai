namespace Pooshit.Ai.Net.DynamicFF;

/// <summary>
/// connects neurons in a feed forward net
/// </summary>
public class FFConnection {
    
    /// <summary>
    /// source neuron
    /// </summary>
    public int Source { get; set; }
    
    /// <summary>
    /// target neuron
    /// </summary>
    public int Target { get; set; }
    
    /// <summary>
    /// weight of connection
    /// </summary>
    public float Weight { get; set; }

    /// <summary>
    /// clones this connection
    /// </summary>
    /// <returns>cloned connection</returns>
    public FFConnection Clone() {
        return new() {
                         Source = Source,
                         Target = Target,
                         Weight = Weight
                     };
    }

    /// <summary>
    /// get a hashcode representing the connection data (without weight)
    /// </summary>
    public int StructureHash => HashCode.Combine(Source, Target);

    /// <inheritdoc />
    public override string ToString() {
        return $"{Source} * {Weight} -> {Target}";
    }
}