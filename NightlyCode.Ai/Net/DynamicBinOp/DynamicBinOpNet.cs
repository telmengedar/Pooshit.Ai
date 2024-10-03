namespace NightlyCode.Ai.Net.DynamicBinOp;

public class DynamicBinOpNet : INeuronalNet<DynamicBinOpConfiguration> {
    DynamicBinOpConfiguration configuration;
    float[] neuronValues;
    readonly Dictionary<string, int> named = new();
    
    /// <summary>
    /// creates a new <see cref="DynamicBinOpNet"/>
    /// </summary>
    /// <param name="configuration">net configuration</param>
    public DynamicBinOpNet(DynamicBinOpConfiguration configuration) {
        this.configuration = configuration;
        foreach (NamedNeuronConfig input in configuration.Inputs)
            named[input.Name] = input.Index;
        foreach (NamedTargetNeuronConfig output in configuration.Outputs)
            named[output.Name] = output.Index;

        neuronValues = new float[configuration.Inputs.Length + configuration.Outputs.Length + configuration.Neurons.Length];
    }

    /// <summary>
    /// indexer for values
    /// </summary>
    /// <param name="name"></param>
    public float this[string name] {
        get => neuronValues[named[name]];
        set => neuronValues[named[name]] = value;
    }

    float this[int index] {
        get => neuronValues[index];
        set => neuronValues[index] = value;
    }

    /// <summary>
    /// input names
    /// </summary>
    public IEnumerable<string> Inputs => configuration.Inputs.Select(i => i.Name);

    /// <summary>
    /// output names
    /// </summary>
    public IEnumerable<string> Outputs => configuration.Outputs.Select(i => i.Name);

    /// <inheritdoc />
    public void Compute() {
        foreach (IGrouping<int, BinOpConnection> group in configuration.GroupedConnections) {
            TargetNeuronConfig targetConfig=configuration.GetTargetNeuron(group.Key);

            this[group.Key] = group.Select(g => {
                                               if (g.Rhs == -1)
                                                   return this[g.Lhs] * g.Weight;

                                               return NMath.Compute(this[g.Lhs],
                                                                    this[g.Rhs],
                                                                    g.Operation) * g.Weight;
                                           }).Aggregate(targetConfig.Aggregate)
                                   .Activation(targetConfig.Activation);
        }
    }
    
    /// <inheritdoc />
    public void Update(DynamicBinOpConfiguration configuration) {
        this.configuration = configuration;

        if (neuronValues.Length < this.configuration.Inputs.Length + this.configuration.Outputs.Length + this.configuration.Neurons.Length)
            neuronValues = new float[this.configuration.Inputs.Length + this.configuration.Outputs.Length + this.configuration.Neurons.Length];
        else 
            foreach (NamedTargetNeuronConfig output in configuration.Outputs)
                this[output.Name] = 0.0f;
    }
}