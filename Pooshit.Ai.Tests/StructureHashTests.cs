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
    [Ignore("DiVoid #9043 - StructureHash ignores neuron configuration (aggregate/activation), collapsing structural diversity across neuron-config variants")]
    public void StructureHash_BOConfiguration_DifferentNeuronConfiguration_IntendedToProduceDifferentHash() {
        DynamicBOConfiguration lhs = new(Neurons(AggregateType.Sum, ActivationFunc.None), [new() { Lhs = 0, Rhs = 1, Target = 2, Operation = OperationType.Multiply, Weight = 1.0f }]);
        DynamicBOConfiguration rhs = new(Neurons(AggregateType.Max, ActivationFunc.Tanh), [new() { Lhs = 0, Rhs = 1, Target = 2, Operation = OperationType.Multiply, Weight = 1.0f }]);

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
    [Ignore("DiVoid #9043 - StructureHash ignores neuron configuration (aggregate/activation), collapsing structural diversity across neuron-config variants")]
    public void StructureHash_FFConfiguration_DifferentNeuronConfiguration_IntendedToProduceDifferentHash() {
        DynamicFFConfiguration lhs = new(Neurons(AggregateType.Sum, ActivationFunc.None), [new() { Source = 0, Target = 2, Weight = 1.0f }]);
        DynamicFFConfiguration rhs = new(Neurons(AggregateType.Max, ActivationFunc.Tanh), [new() { Source = 0, Target = 2, Weight = 1.0f }]);

        Assert.That(lhs.StructureHash(), Is.Not.EqualTo(rhs.StructureHash()));
    }
}
