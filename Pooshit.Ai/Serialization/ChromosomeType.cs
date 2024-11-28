namespace Pooshit.Ai.Serialization;

/// <summary>
/// type of serialized chromosome
/// </summary>
public enum ChromosomeType : byte {
    
    /// <summary>
    /// dynamic binary operations
    /// </summary>
    DynamicBO,
    
    /// <summary>
    /// dynamic feed forward
    /// </summary>
    DynamicFF
}