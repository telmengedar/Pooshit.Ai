using Pooshit.Ai.Net;

namespace NightlyCode.Ai.Tests;

class FakeNet : INeuronalNet<FakeChromosome> {
    readonly Dictionary<int, float> values = new();

    public FakeNet(FakeChromosome chromosome) { }

    public float this[string name] {
        get => 0.0f;
        set { }
    }


    public float this[int index] {
        get => values.GetValueOrDefault(index);
        set => values[index] = value;
    }


    public void Compute() { }


    public void SetInputValues(float[] inputValues) { }


    public void Update(FakeChromosome configuration) { }
}
