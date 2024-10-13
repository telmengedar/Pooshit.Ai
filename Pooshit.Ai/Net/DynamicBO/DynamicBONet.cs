using Pooshit.Ai.Extensions;
using Pooshit.Ai.Neurons;

namespace Pooshit.Ai.Net.DynamicBO;

public class DynamicBONet : INeuronalNet<DynamicBOConfiguration> {
    DynamicBOConfiguration configuration;
    float[] neuronValues;
    readonly Dictionary<string, int> named = new();
    
    /// <summary>
    /// creates a new <see cref="DynamicBONet"/>
    /// </summary>
    /// <param name="configuration">net configuration</param>
    public DynamicBONet(DynamicBOConfiguration configuration) {
        this.configuration = configuration;
        foreach (NeuronConfig input in configuration.Neurons) {
            if(!string.IsNullOrEmpty(input.Name))
                named[input.Name] = input.Index;
        }
        
        neuronValues = new float[configuration.Neurons.Length];
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
    /// set unnamed input values
    /// </summary>
    /// <param name="values">values to set</param>
    /// <exception cref="ArgumentException">thrown when length of value array doesn't match input neuron count</exception>
    public void SetInputValues(float[] values) {
        if (values.Length != configuration.InputCount)
            throw new ArgumentException("Invalid number of values");
        Array.Copy(values, neuronValues, values.Length);
    }
    
    /// <summary>
    /// input names
    /// </summary>
    public IEnumerable<string> Inputs => configuration.Neurons.Take(configuration.InputCount).Select(i => i.Name);

    /// <summary>
    /// output names
    /// </summary>
    public IEnumerable<string> Outputs => configuration.Neurons.Skip(configuration.InputCount)
                                                       .Take(configuration.OutputCount)
                                                       .Select(i => i.Name);

    /// <inheritdoc />
    public void Compute() {
        foreach (IGrouping<int, BOConnection> group in configuration.GroupedConnections) {
            NeuronConfig targetConfig=configuration.GetTargetNeuron(group.Key);

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
    public void Update(DynamicBOConfiguration configuration) {
        this.configuration = configuration;

        if (neuronValues.Length < this.configuration.Neurons.Length)
            Array.Resize(ref neuronValues, configuration.Neurons.Length);
        else 
            foreach (NeuronConfig output in configuration.Neurons.Skip(configuration.InputCount).Take(configuration.OutputCount))
                this[output.Name] = 0.0f;
    }
}