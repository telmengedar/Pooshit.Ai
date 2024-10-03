namespace NightlyCode.Ai.Net;

/// <summary>
/// neuronal net
/// </summary>
/// <typeparam name="T">type of neuronal configuration</typeparam>
public interface INeuronalNet<T> {
    
    /// <summary>
    /// computes the state of the neuronal net
    /// </summary>
    void Compute();

    /// <summary>
    /// updates connection weights of neuronal net
    /// </summary>
    /// <param name="configuration">configuration containing weights</param>
    void Update(T configuration);
}