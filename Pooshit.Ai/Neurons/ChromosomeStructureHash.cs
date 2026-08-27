namespace Pooshit.Ai.Neurons;

/// <summary>
/// combines a chromosome's neuron and connection hashes into one structure hash
/// </summary>
public static class ChromosomeStructureHash {

    /// <summary>
    /// combines neuron and connection structure hashes
    /// </summary>
    /// <param name="neurons">neurons of the chromosome, folded in their canonical index order</param>
    /// <param name="connectionHashes">structure hash of every connection, folded order-invariant</param>
    /// <returns>combined structure hash</returns>
    public static int Combine(IEnumerable<NeuronConfig> neurons, IEnumerable<int> connectionHashes) {
        HashCode hash = new();
        foreach (NeuronConfig neuron in neurons)
            hash.Add(neuron.StructureHash);

        int connectionHash = 0;
        foreach (int connectionStructureHash in connectionHashes)
            connectionHash += connectionStructureHash;
        hash.Add(connectionHash);

        return hash.ToHashCode();
    }
}
