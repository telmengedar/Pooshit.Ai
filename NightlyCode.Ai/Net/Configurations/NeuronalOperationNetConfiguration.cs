using NightlyCode.Ai.Extern;
using NightlyCode.Ai.Genetics;
using NightlyCode.Ai.Genetics.Mutation;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.Configurations;

public class NeuronalOperationNetConfiguration : ICrossChromosome<NeuronalOperationNetConfiguration> {
    
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
    /// operations
    /// </summary>
    public NeuronalOperation[] Operations { get; set; }

    /// <summary>
    /// operation groups to compute
    /// </summary>
    public NeuronalOperationGroup[] OperationGroups { get; set; }

    /// <summary>
    /// creates a new <see cref="NeuronalOperationNetConfiguration"/>
    /// </summary>
    /// <param name="inputs">input names</param>
    /// <param name="outputs">output names</param>
    /// <param name="layers">number of neuronal layers</param>
    /// <param name="layerSize">size of neuronal layers</param>
    /// <param name="operations">operations</param>
    /// <param name="operationGroups">operation groups</param>
    public NeuronalOperationNetConfiguration(string[] inputs, string[] outputs, int layers, int layerSize, NeuronalOperation[] operations, NeuronalOperationGroup[] operationGroups) {
        Inputs = inputs;
        Outputs = outputs;
        Layers = layers;
        LayerSize = layerSize;
        Operations = operations;
        OperationGroups = operationGroups;
    }

    /// <summary>
    /// creates a new <see cref="NeuronalOperationNetConfiguration"/>
    /// </summary>
    /// <param name="inputs">input names</param>
    /// <param name="outputs">output names</param>
    /// <param name="layers">number of neuronal layers</param>
    /// <param name="layerSize">size of neuronal layers</param>
    /// <param name="setup">setup used for initialization</param>
    public NeuronalOperationNetConfiguration(string[] inputs, string[] outputs, int layers, int layerSize, CrossSetup setup=null) {
        setup ??= new CrossSetup();
        setup.Rng ??= new Rng();
        setup.OperationTypes ??= new OperationTypeOptions(new MutationEntry<OperationType>(OperationType.Add, 1.0));
        setup.AggregateTypes ??= new AggregateTypeOptions(new MutationEntry<AggregateType>(AggregateType.Average, 1.0));
        setup.ActivationFuncs ??= new ActivationFuncOptions(new MutationEntry<ActivationFunc>(ActivationFunc.None, 1.0));
        
        Inputs = inputs;
        Outputs = outputs;
        Layers = layers;
        LayerSize = layerSize;

        List<NeuronalOperation> operations = new();
        for (int li = 0; li < layerSize; ++li) {
            for (int lhsi = 0; lhsi < inputs.Length;++lhsi) {
                operations.Add(new NeuronalOperation {
                                                         Lhs = new NeuronIndex(NeuronIndex.InputLayer, lhsi),
                                                         Output = new NeuronIndex(0, li),
                                                         Weight = setup.NextWeight(),
                                                     });
                if (lhsi < inputs.Length - 1) {
                    for (int rhsi = 1; rhsi < inputs.Length; ++rhsi)
                        operations.Add(new NeuronalOperation {
                                                                 Operation = setup.NextOperation(),
                                                                 Lhs = new NeuronIndex(NeuronIndex.InputLayer, lhsi),
                                                                 Rhs = new NeuronIndex(NeuronIndex.InputLayer, rhsi),
                                                                 Output = new NeuronIndex(0, li),
                                                                 Weight = setup.NextWeight(),
                                                             });
                }
            }
        }

        for (int layer = 0; layer < layers - 1; ++layer) {
            for (int li = 0; li < layerSize; ++li) {
                for (int lhsi = 0; lhsi < layerSize; ++lhsi) {
                    operations.Add(new NeuronalOperation {
                                                             Lhs = new NeuronIndex(layer, lhsi),
                                                             Output = new NeuronIndex(layer + 1, li),
                                                             Weight = setup.NextWeight(),
                                                         });
                    if (lhsi < layerSize - 1) {
                        for (int rhsi = 1; rhsi < layerSize; ++rhsi) {
                            operations.Add(new NeuronalOperation {
                                                                     Operation = setup.NextOperation(),
                                                                     Lhs = new NeuronIndex(layer, lhsi),
                                                                     Rhs = new NeuronIndex(layer, rhsi),
                                                                     Output = new NeuronIndex(layer + 1, li),
                                                                     Weight = setup.NextWeight(),
                                                                 });
                        }
                    }
                }
            }
        }

        for (int li = 0; li < outputs.Length; ++li) {
            for (int lhsi = 0; lhsi < layerSize; ++lhsi) {
                operations.Add(new NeuronalOperation {
                                                         Lhs = new NeuronIndex(layers - 1, lhsi),
                                                         Output = new NeuronIndex(layers - 1, li),
                                                         Weight = setup.NextWeight(),
                                                     });
                if (lhsi < layerSize - 1) {
                    for (int rhsi = 1; rhsi < layerSize; ++rhsi) {
                        operations.Add(new NeuronalOperation {
                                                                 Operation = setup.NextOperation(),
                                                                 Lhs = new NeuronIndex(layers - 1, lhsi),
                                                                 Rhs = new NeuronIndex(layers - 1, rhsi),
                                                                 Output = new NeuronIndex(NeuronIndex.OutputLayer, li),
                                                                 Weight = setup.NextWeight(),
                                                             });
                    }
                }
            }
        }

        Operations = operations.ToArray();
        OperationGroups = operations.GroupBy(o => o.Output)
                                    .OrderBy(g => g.Key.Layer)
                                    .Select(group => new NeuronalOperationGroup {
                                                                                    Operations = group.ToArray(),
                                                                                    Output = group.Key,
                                                                                    Aggregate = setup.NextAggregate()
                                                                                }).ToArray();
    }

    /// <inheritdoc />
    public NeuronalOperationNetConfiguration Clone() {
        NeuronalOperation[] connections = new NeuronalOperation[Operations.Length];
        for (int i = 0; i < Operations.Length; ++i)
            connections[i] = Operations[i].Clone();

        return new NeuronalOperationNetConfiguration(Inputs, Outputs, Layers, LayerSize, connections, OperationGroups);
    }

    public NeuronalOperationNetConfiguration Cross(NeuronalOperationNetConfiguration other, CrossSetup setup) {
        setup.Rng ??= new();
        setup.OperationTypes ??= new();
        setup.AggregateTypes ??= new();
        setup.ActivationFuncs ??= new();

        NeuronalOperation[] connections = new NeuronalOperation[Operations.Length];
        for (int i = 0; i < Operations.Length; ++i)
            connections[i] = setup.Rng.NextDouble() < 0.5 ? Operations[i] : other.Operations[i];


        NeuronalOperationGroup[] groups = connections.GroupBy(o => o.Output)
                                                     .OrderBy(g => g.Key.Layer)
                                                     .Select(group => new NeuronalOperationGroup {
                                                                                                     Operations = group.ToArray(),
                                                                                                     Output = group.Key
                                                                                                 }).ToArray();

        for (int i = 0; i < groups.Length; ++i)
            groups[i].Aggregate = setup.Rng.NextDouble() < 0.5 ? OperationGroups[i].Aggregate : other.OperationGroups[i].Aggregate;

        if (setup.Rng.NextDouble() < setup.MutateChance) {
            switch (setup.Rng.NextInt(3)) {
                default:
                case 0: {
                    int connectionIndex = setup.Rng.NextInt(connections.Length);
                    connections[connectionIndex] = connections[connectionIndex].Clone();
                    connections[connectionIndex].Weight += setup.NextWeight();
                    break;
                }
                case 1: {
                    int connectionIndex = setup.Rng.NextInt(connections.Length);
                    connections[connectionIndex] = connections[connectionIndex].Clone();
                    connections[connectionIndex].Operation = setup.OperationTypes.SelectItem(setup.Rng);
                    //connections[connectionIndex].Weight = setup.NextWeight();
                    break;
                }
                case 2:
                    int mutateCount = Math.Max(1, (int)(OperationGroups.Length * setup.MutateRate));
                    for (int i = 0; i < mutateCount; ++i)
                        groups[setup.Rng.NextInt(groups.Length)].Aggregate = setup.AggregateTypes.SelectItem(setup.Rng);
                    break;
            }
        }

        return new(Inputs, Outputs, Layers, LayerSize, connections, groups);
    }

    public void Randomize(CrossSetup setup) {
        setup ??= new();
        setup.Rng ??= new();
        setup.OperationTypes ??= new();
        setup.AggregateTypes ??= new();
        setup.ActivationFuncs ??= new();

        NeuronalOperation[] connections = new NeuronalOperation[Operations.Length];
        for (int i = 0; i < Operations.Length; ++i)
            connections[i] = Operations[i].Clone();

        Operations = connections;
        OperationGroups = connections.GroupBy(o => o.Output)
                                     .OrderBy(g => g.Key.Layer)
                                     .Select(group => new NeuronalOperationGroup {
                                                                                     Operations = group.ToArray(),
                                                                                     Output = group.Key,
                                                                                     Aggregate = setup.NextAggregate()
                                                                                 }).ToArray();
    }
    
    public void SetWeight(int index, float weight) {
        Operations[index].Weight = weight;
    }

    public float GetWeight(int index) {
        return Operations[index].Weight;
    }
    
    public int StructureHash() {
        return (int)Operations.Select(o => o.GetHashCode()).GenerateHash();
    }

    /// <inheritdoc />
    public float FitnessModifier => 1.0f;
}