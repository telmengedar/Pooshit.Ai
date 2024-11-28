namespace Pooshit.Ai.Serialization;

/// <summary>
/// type of following chunk
/// </summary>
public enum SerializationChunk : short {
    
    /// <summary>
    /// type of serialized package
    /// </summary>
    Type=1,
    
    /// <summary>
    /// chromosome data
    /// </summary>
    Chromosomes=1,
    
    /// <summary>
    /// serialized input neurons
    /// </summary>
    InputNeurons=2,
    
    /// <summary>
    /// serialized output neurons
    /// </summary>
    OutputNeurons=3,
    
    /// <summary>
    /// serialized generated neurons
    /// </summary>
    Neurons=4,
    
    /// <summary>
    /// serialized connections
    /// </summary>
    Connections=5,
    
    /// <summary>
    /// input neurons with generators
    /// </summary>
    InputGenerators=6
}