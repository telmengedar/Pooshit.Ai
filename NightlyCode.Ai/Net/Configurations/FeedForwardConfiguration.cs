using NightlyCode.Ai.Genetics;
using NightlyCode.Ai.Genetics.Mutation;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.Configurations;

/// <summary>
/// configuration for a feed forward neuronal network
/// </summary>
public class FeedForwardConfiguration : ICrossChromosome<FeedForwardConfiguration> {
    
    /// <summary>
    /// input neurons
    /// </summary>
    public string[] Inputs { get; set; }
    
    /// <summary>
    /// output neurons
    /// </summary>
    public string[] Outputs { get; set; }
    
    /// <summary>
    /// number of layers
    /// </summary>
    public int Layers { get; set; }
    
    /// <summary>
    /// size of a layer
    /// </summary>
    public int LayerSize { get; set; }

    /// <summary>
    /// connection weights
    /// </summary>
    public float[] Weights { get; set; }

    /// <summary>
    /// neuronal aggregates
    /// </summary>
    public AggregateType[] Aggregates { get; set; }

    /// <summary>
    /// neuronal activation functions
    /// </summary>
    public ActivationFunc[] ActivationFunctions { get; set; }

    /// <summary>
    /// creates a new <see cref="FeedForwardConfiguration"/>
    /// </summary>
    public FeedForwardConfiguration() { }

    /// <summary>
    /// creates a new <see cref="FeedForwardConfiguration"/>
    /// </summary>
    /// <param name="inputs">input parameters</param>
    /// <param name="outputs">output parameters</param>
    /// <param name="layers">number of layers</param>
    /// <param name="layerSize">size of a layer</param>
    public FeedForwardConfiguration(string[] inputs, string[] outputs, int layers, int layerSize, CrossSetup setup=null) {
        setup ??= new();
        setup.Rng ??= new();
        setup.OperationTypes ??= new(new MutationEntry<OperationType>(OperationType.Add, 1.0));
        setup.AggregateTypes ??= new(new MutationEntry<AggregateType>(AggregateType.Average, 1.0));
        setup.ActivationFuncs ??= new(new MutationEntry<ActivationFunc>(ActivationFunc.None, 1.0));

        Inputs = inputs;
        Outputs = outputs;
        Layers = layers;
        LayerSize = layerSize;
        Weights = new float[(Layers - 1) * LayerSize * LayerSize + inputs.Length * LayerSize + outputs.Length * layerSize];
        for (int i = 0; i < Weights.Length; ++i)
            Weights[i] = setup.NextWeight();
        Aggregates = new AggregateType[Layers * LayerSize + outputs.Length];
        for (int i = 0; i < Aggregates.Length; ++i)
            Aggregates[i] = setup.NextAggregate();
        ActivationFunctions = new ActivationFunc[Aggregates.Length];
        for (int i = 0; i < ActivationFunctions.Length; ++i)
            ActivationFunctions[i] = setup.NextFunc();
    }

    /// <summary>
    /// creates a new <see cref="FeedForwardConfiguration"/>
    /// </summary>
    /// <param name="inputs">input parameters</param>
    /// <param name="outputs">output parameters</param>
    /// <param name="layers">number of layers</param>
    /// <param name="layerSize">size of a layer</param>
    /// <param name="weights">input weights</param>
    /// <param name="aggregates">neuron aggregates</param>
    /// <param name="activationActivationFunctions">activation functions of neurons</param>
    public FeedForwardConfiguration(string[] inputs, string[] outputs, int layers, int layerSize, float[] weights, AggregateType[] aggregates, ActivationFunc[] activationActivationFunctions) {
        Inputs = inputs;
        Outputs = outputs;
        Layers = layers;
        LayerSize = layerSize;
        Weights = weights;
        Aggregates = aggregates;
        ActivationFunctions = activationActivationFunctions;
    }

    /// <inheritdoc />
    public FeedForwardConfiguration Clone() {
        return new(Inputs, Outputs, Layers, LayerSize, Weights.ToArray(), Aggregates.ToArray(), ActivationFunctions.ToArray());
    }

    /// <inheritdoc />
    public FeedForwardConfiguration Cross(FeedForwardConfiguration other, CrossSetup setup) {
        setup.Rng ??= new();
        setup.OperationTypes ??= new();
        setup.AggregateTypes ??= new();
        setup.ActivationFuncs ??= new();

        float[] weights = new float[Weights.Length];
        AggregateType[] aggregates = new AggregateType[Aggregates.Length];
        ActivationFunc[] funcs = new ActivationFunc[ActivationFunctions.Length];
        
        for (int i = 0; i < Weights.Length; i++) {
            if (setup.Rng.NextDouble() < 0.5)
                weights[i] = Weights[i];
            else
                weights[i] = other.Weights[i];
        }
        for (int i = 0; i < Aggregates.Length; i++) {
            if (setup.Rng.NextDouble() < 0.5)
                aggregates[i] = Aggregates[i];
            else
                aggregates[i] = other.Aggregates[i];
        }
        for (int i = 0; i < ActivationFunctions.Length; i++) {
            if (setup.Rng.NextDouble() < 0.5)
                funcs[i] = ActivationFunctions[i];
            else
                funcs[i] = other.ActivationFunctions[i];
        }

        if (setup.Rng.NextDouble() < setup.MutateChance) {
            int count;
            switch (setup.Rng.NextInt(3)) {
                default:
                case 0:
                    count = Math.Max(1, (int)(Weights.Length * setup.MutateRate));
                    for (int i = 0; i < count; ++i)
                        Weights[setup.Rng.NextInt(Weights.Length)] += setup.NextWeight();
                    break;
                case 1:
                    count = Math.Max(1, (int)(ActivationFunctions.Length * setup.MutateRate));
                    for (int i = 0; i < count; ++i)
                        ActivationFunctions[setup.Rng.NextInt(ActivationFunctions.Length)] = setup.NextFunc();
                    break;
                case 2:
                    count = Math.Max(1, (int)(Aggregates.Length * setup.MutateRate));
                    for (int i = 0; i < count; ++i)
                        Aggregates[setup.Rng.NextInt(Aggregates.Length)] = setup.NextAggregate();
                    break;
            }
        }
        
        return new(Inputs, Outputs, Layers, LayerSize) { Weights = weights, Aggregates = aggregates, ActivationFunctions = funcs };
    }

    /// <inheritdoc />
    public void Randomize(CrossSetup setup) {
        setup ??= new();
        setup.Rng ??= new();
        setup.OperationTypes ??= new();
        setup.AggregateTypes ??= new();
        setup.ActivationFuncs ??= new();

        for (int i = 0; i < Weights.Length; ++i)
            Weights[i] = setup.NextWeight();
        for (int i = 0; i < ActivationFunctions.Length; ++i)
            ActivationFunctions[i] = setup.NextFunc();
        for (int i = 0; i < Aggregates.Length; ++i)
            Aggregates[i] = setup.NextAggregate();
    }

    public int StructureHash() {
        int hash = 0;
        foreach (AggregateType aggregate in Aggregates) {
            hash *= 397;
            hash ^= aggregate.GetHashCode();
        }

        foreach (ActivationFunc func in ActivationFunctions) {
            hash *= 397;
            hash ^= func.GetHashCode();
        }
        return hash;
    }

    /// <inheritdoc />
    public float FitnessModifier => 1.0f;
}