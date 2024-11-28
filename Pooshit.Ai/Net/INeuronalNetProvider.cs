using Pooshit.Ai.Net.DynamicBO;

namespace Pooshit.Ai.Net;

/// <summary>
/// provides neuronal nets
/// </summary>
/// <typeparam name="T">type of neuronal configuration</typeparam>
public interface INeuronalNetProvider<T> {

    /// <summary>
    /// get a neuronal net
    /// </summary>
    /// <returns>neuronal net to be used for computational purposes</returns>
    Task<INeuronalNet<T>> Get(DynamicBOConfiguration configuration);
}