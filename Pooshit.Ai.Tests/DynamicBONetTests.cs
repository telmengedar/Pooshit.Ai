using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.Operations;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class DynamicBONetTests {

    static DynamicBONet BuildNet(BOConnection[] connections) {
        NeuronConfig[] neurons = [
            new() { Name = "huge", Index = 0, OrderNumber = 0.0f },
            new() { Name = "unit", Index = 1, OrderNumber = 0.0f },
            new() { Name = "five", Index = 2, OrderNumber = 0.0f },
            new() { Name = "out", Index = 3, OrderNumber = 1.0f, Aggregate = AggregateType.Sum, Activation = ActivationFunc.None }
        ];
        DynamicBOConfiguration configuration = new(neurons, connections);
        DynamicBONet net = new(configuration);
        net.SetInputValues([3e38f, 1.0f, 5.0f]);
        return net;
    }


    [Test, Parallelizable]
    [Description("The executable statement of DiVoid #9772: a connection whose finite Compute result overflows to Infinity after the weight multiply must not poison the whole neuron - without the guard this settles to [Infinity, 5].Sum.Activation(None) == 0")]
    public void Compute_OperationResultOverflowsAfterWeightMultiply_DoesNotZeroWholeNeuron() {
        BOConnection[] connections = [
            new() { Lhs = 0, Rhs = 1, Target = 3, Operation = OperationType.Multiply, Weight = 1e10f },
            new() { Lhs = 2, Rhs = -1, Target = 3, Weight = 1.0f }
        ];
        DynamicBONet net = BuildNet(connections);

        net.Compute();

        Assert.That(net["out"], Is.EqualTo(5.0f));
    }


    [Test, Parallelizable]
    [Description("The Rhs == -1 passthrough branch never calls NMath.Compute, so it carries its own weight-multiply overflow independent of Compute's guard - same containment property as the operation branch above")]
    public void Compute_PassthroughConnectionOverflowsAfterWeightMultiply_DoesNotZeroWholeNeuron() {
        BOConnection[] connections = [
            new() { Lhs = 0, Rhs = -1, Target = 3, Weight = 1e10f },
            new() { Lhs = 2, Rhs = -1, Target = 3, Weight = 1.0f }
        ];
        DynamicBONet net = BuildNet(connections);

        net.Compute();

        Assert.That(net["out"], Is.EqualTo(5.0f));
    }


    [Test, Parallelizable]
    [Description("R1 sibling of the two containment tests above: two ordinary, non-overflowing connections must sum normally, proving the guard fires only for non-finite values and not for every connection")]
    public void Compute_NoConnectionOverflows_SumsBothContributions() {
        BOConnection[] connections = [
            new() { Lhs = 1, Rhs = -1, Target = 3, Weight = 3.0f },
            new() { Lhs = 2, Rhs = -1, Target = 3, Weight = 1.0f }
        ];
        DynamicBONet net = BuildNet(connections);

        net.Compute();

        Assert.That(net["out"], Is.EqualTo(8.0f));
    }
}
