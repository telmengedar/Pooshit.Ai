using Pooshit.Ai.Net.DynamicFF;
using Pooshit.Ai.Net.Operations;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class DynamicFFNetTests {

    static DynamicFFNet BuildNet(FFConnection[] connections) {
        NeuronConfig[] neurons = [
            new() { Name = "huge", Index = 0, OrderNumber = 0.0f },
            new() { Name = "five", Index = 1, OrderNumber = 0.0f },
            new() { Name = "out", Index = 2, OrderNumber = 1.0f, Aggregate = AggregateType.Sum, Activation = ActivationFunc.None }
        ];
        DynamicFFConfiguration configuration = new(neurons, connections);
        DynamicFFNet net = new(configuration);
        net.SetInputValues([3e38f, 5.0f]);
        return net;
    }


    [Test, Parallelizable]
    [Description("The executable statement of DiVoid #9772 for the FF family: DynamicFFNet never calls NMath.Compute, so its connection product neurons[Source] * Weight was entirely unguarded - without the guard this settles to [Infinity, 5].Sum.Activation(None) == 0")]
    public void Compute_ConnectionProductOverflows_DoesNotZeroWholeNeuron() {
        FFConnection[] connections = [
            new() { Source = 0, Target = 2, Weight = 1e10f },
            new() { Source = 1, Target = 2, Weight = 1.0f }
        ];
        DynamicFFNet net = BuildNet(connections);

        net.Compute();

        Assert.That(net["out"], Is.EqualTo(5.0f));
    }


    [Test, Parallelizable]
    [Description("R1 sibling of the containment test above: two ordinary, non-overflowing connections must sum normally, proving the guard fires only for non-finite values and not for every connection")]
    public void Compute_NoConnectionOverflows_SumsBothContributions() {
        FFConnection[] connections = [
            new() { Source = 1, Target = 2, Weight = 3.0f },
            new() { Source = 1, Target = 2, Weight = 1.0f }
        ];
        DynamicFFNet net = BuildNet(connections);

        net.Compute();

        Assert.That(net["out"], Is.EqualTo(20.0f));
    }
}
