using NightlyCode.Ai.Neurons;

namespace NightlyCode.Ai.Net;

/// <summary>
/// neuronal net
/// </summary>
/// <typeparam name="T">type of neuronal configuration</typeparam>
public interface INeuronalNet<T> {

    /// <summary>
    /// input neurons
    /// </summary>
    NamedNeurons Input { get; }

    /// <summary>
    /// neurons holding result
    /// </summary>
    NamedNeurons Output { get; }

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