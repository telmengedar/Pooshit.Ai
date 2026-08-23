using Pooshit.Ai.Net;

namespace NightlyCode.Ai.Tests;

class FakeNet : INeuronalNet<FakeChromosome> {
    readonly Dictionary<int, float> values = new();

    public FakeNet(FakeChromosome chromosome) { }

    public List<float[]> InputValues { get; } = new();

    public List<(int Index, float Value)> IndexWrites { get; } = new();

    public List<FakeChromosome> Updates { get; } = new();

    public float this[string name] {
        get => throw new NotSupportedException($"{nameof(FakeNet)} resolves neurons by index; string lookup for '{name}' was not expected");
        set => throw new NotSupportedException($"{nameof(FakeNet)} resolves neurons by index; string lookup for '{name}' was not expected");
    }


    public float this[int index] {
        get => values.GetValueOrDefault(index);
        set {
            IndexWrites.Add((index, value));
            values[index] = value;
        }
    }


    public void Compute() { }


    public void SetInputValues(float[] inputValues) => InputValues.Add(inputValues);


    public void Update(FakeChromosome configuration) => Updates.Add(configuration);
}
