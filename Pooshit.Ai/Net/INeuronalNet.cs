namespace Pooshit.Ai.Net;

/// <summary>
/// neuronal net
/// </summary>
/// <typeparam name="T">type of neuronal configuration</typeparam>
public interface INeuronalNet<T> {
    
    /// <summary>
    /// access to neurons
    /// </summary>
    /// <param name="name">name of neuron</param>
    float this[string name] { get; set; }
    
    /// <summary>
    /// computes the state of the neuronal net
    /// </summary>
    void Compute();

    /// <summary>
    /// set an array of unnamed input values
    /// </summary>
    /// <param name="values">values to set</param>
    void SetInputValues(float[] values);
    
    /// <summary>
    /// updates connection weights of neuronal net
    /// </summary>
    /// <param name="configuration">configuration containing weights</param>
    void Update(T configuration);
}