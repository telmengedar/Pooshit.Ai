using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.DynamicFF;
using Pooshit.Ai.Net.Operations;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class StructureHashTests {

    static NeuronConfig[] Neurons(AggregateType outputAggregate = AggregateType.Sum, ActivationFunc outputActivation = ActivationFunc.None) => [
        new() { Index = 0, OrderNumber = 0.0f },
        new() { Index = 1, OrderNumber = 0.0f },
        new() { Index = 2, OrderNumber = 1.0f, Aggregate = outputAggregate, Activation = outputActivation }
    ];


    [Test, Parallelizable]
    public void StructureHash_BOConfiguration_SameConnectionTopologyDifferentWeights_ProducesEqualHash() {
        DynamicBOConfiguration lhs = new(Neurons(), [new() { Lhs = 0, Rhs = 1, Target = 2, Operation = OperationType.Multiply, Weight = 1.0f }]);
        DynamicBOConfiguration rhs = new(Neurons(), [new() { Lhs = 0, Rhs = 1, Target = 2, Operation = OperationType.Multiply, Weight = 99.0f }]);

        Assert.That(lhs.StructureHash(), Is.EqualTo(rhs.StructureHash()));
    }


    [Test, Parallelizable]
    public void StructureHash_BOConfiguration_DifferentConnectionTopology_ProducesDifferentHash() {
        DynamicBOConfiguration lhs = new(Neurons(), [new() { Lhs = 0, Rhs = 1, Target = 2, Operation = OperationType.Multiply, Weight = 1.0f }]);
        DynamicBOConfiguration rhs = new(Neurons(), [new() { Lhs = 0, Rhs = 1, Target = 2, Operation = OperationType.Add, Weight = 1.0f }]);

        Assert.That(lhs.StructureHash(), Is.Not.EqualTo(rhs.StructureHash()));
    }


    [Test, Parallelizable]
    [Description("DiVoid #9043: pins the intended contract that neuron Aggregate/Activation participate in the structure hash. Verified two-sided (R5): red against the commented-out neuron fold, green once ChromosomeStructureHash.Combine folds NeuronConfig.StructureHash back in.")]
    public void StructureHash_BOConfiguration_DifferentNeuronConfiguration_IntendedToProduceDifferentHash() {
        DynamicBOConfiguration lhs = new(Neurons(AggregateType.Sum, ActivationFunc.None), [new() { Lhs = 0, Rhs = 1, Target = 2, Operation = OperationType.Multiply, Weight = 1.0f }]);
        DynamicBOConfiguration rhs = new(Neurons(AggregateType.Max, ActivationFunc.Tanh), [new() { Lhs = 0, Rhs = 1, Target = 2, Operation = OperationType.Multiply, Weight = 1.0f }]);

        Assert.That(lhs.StructureHash(), Is.Not.EqualTo(rhs.StructureHash()));
    }


    [Test, Parallelizable]
    [Description("DiVoid #9043 coverage item 2: a structural fingerprint must be order-invariant. The same two connections listed in opposite order must hash equal; today's positional hash*=397 fold is order-sensitive and fails this. Sibling to DifferentConnectionTopology above (R1): that test moves the connection SET, this one moves only the LIST ORDER of an unchanged set.")]
    public void StructureHash_BOConfiguration_SameConnectionsListedInDifferentOrder_ProducesEqualHash() {
        BOConnection first = new() { Lhs = 0, Rhs = 1, Target = 2, Operation = OperationType.Multiply, Weight = 1.0f };
        BOConnection second = new() { Lhs = 1, Rhs = 0, Target = 2, Operation = OperationType.Add, Weight = 1.0f };

        DynamicBOConfiguration lhs = new(Neurons(), [first, second]);
        DynamicBOConfiguration rhs = new(Neurons(), [second, first]);

        Assert.That(lhs.StructureHash(), Is.EqualTo(rhs.StructureHash()));
    }


    [Test, Parallelizable]
    [Description("DiVoid #9043 coverage item 3: two structurally different fresh (zero-connection) genomes must not collide. Today's fold only walks Connections, so an empty connection list always hashes to 0 regardless of neuron configuration - every fresh genome collides with every other. Restoring the neuron fold fixes this as a side effect: neurons are always present, even with zero connections.")]
    public void StructureHash_BOConfiguration_TwoDistinctZeroConnectionGenomes_ProduceDifferentHash() {
        DynamicBOConfiguration lhs = new(Neurons(AggregateType.Sum, ActivationFunc.None), []);
        DynamicBOConfiguration rhs = new(Neurons(AggregateType.Max, ActivationFunc.Tanh), []);

        Assert.That(lhs.StructureHash(), Is.Not.EqualTo(rhs.StructureHash()));
    }


    [Test, Parallelizable]
    public void StructureHash_FFConfiguration_SameConnectionTopologyDifferentWeights_ProducesEqualHash() {
        DynamicFFConfiguration lhs = new(Neurons(), [new() { Source = 0, Target = 2, Weight = 1.0f }]);
        DynamicFFConfiguration rhs = new(Neurons(), [new() { Source = 0, Target = 2, Weight = 99.0f }]);

        Assert.That(lhs.StructureHash(), Is.EqualTo(rhs.StructureHash()));
    }


    [Test, Parallelizable]
    public void StructureHash_FFConfiguration_DifferentConnectionTopology_ProducesDifferentHash() {
        DynamicFFConfiguration lhs = new(Neurons(), [new() { Source = 0, Target = 2, Weight = 1.0f }]);
        DynamicFFConfiguration rhs = new(Neurons(), [new() { Source = 1, Target = 2, Weight = 1.0f }]);

        Assert.That(lhs.StructureHash(), Is.Not.EqualTo(rhs.StructureHash()));
    }


    [Test, Parallelizable]
    [Description("DiVoid #9043: pins the intended contract that neuron Aggregate/Activation participate in the structure hash. Verified two-sided (R5): red against the connection-only fold, green once ChromosomeStructureHash.Combine folds NeuronConfig.StructureHash back in.")]
    public void StructureHash_FFConfiguration_DifferentNeuronConfiguration_IntendedToProduceDifferentHash() {
        DynamicFFConfiguration lhs = new(Neurons(AggregateType.Sum, ActivationFunc.None), [new() { Source = 0, Target = 2, Weight = 1.0f }]);
        DynamicFFConfiguration rhs = new(Neurons(AggregateType.Max, ActivationFunc.Tanh), [new() { Source = 0, Target = 2, Weight = 1.0f }]);

        Assert.That(lhs.StructureHash(), Is.Not.EqualTo(rhs.StructureHash()));
    }


    [Test, Parallelizable]
    [Description("DiVoid #9043 coverage item 2: a structural fingerprint must be order-invariant. The same two connections listed in opposite order must hash equal; today's positional hash*=397 fold is order-sensitive and fails this. Sibling to DifferentConnectionTopology above (R1): that test moves the connection SET, this one moves only the LIST ORDER of an unchanged set.")]
    public void StructureHash_FFConfiguration_SameConnectionsListedInDifferentOrder_ProducesEqualHash() {
        FFConnection first = new() { Source = 0, Target = 2, Weight = 1.0f };
        FFConnection second = new() { Source = 1, Target = 2, Weight = 1.0f };

        DynamicFFConfiguration lhs = new(Neurons(), [first, second]);
        DynamicFFConfiguration rhs = new(Neurons(), [second, first]);

        Assert.That(lhs.StructureHash(), Is.EqualTo(rhs.StructureHash()));
    }


    [Test, Parallelizable]
    [Description("DiVoid #9043 coverage item 3: two structurally different fresh (zero-connection) genomes must not collide. Today's fold only walks Connections, so an empty connection list always hashes to 0 regardless of neuron configuration - every fresh genome collides with every other. Restoring the neuron fold fixes this as a side effect: neurons are always present, even with zero connections.")]
    public void StructureHash_FFConfiguration_TwoDistinctZeroConnectionGenomes_ProduceDifferentHash() {
        DynamicFFConfiguration lhs = new(Neurons(AggregateType.Sum, ActivationFunc.None), []);
        DynamicFFConfiguration rhs = new(Neurons(AggregateType.Max, ActivationFunc.Tanh), []);

        Assert.That(lhs.StructureHash(), Is.Not.EqualTo(rhs.StructureHash()));
    }
}
