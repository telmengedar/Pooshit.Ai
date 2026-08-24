using Pooshit.Ai.Net;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// a constant-zero oracle net on purpose (R3, test README): output queries always resolve to
/// whatever was last written to that index, defaulting to 0 for anything never written.
/// Records every <see cref="SetInputValues"/> array and <see cref="Update"/> configuration it
/// receives, since both sit on <see cref="Pooshit.Ai.Net.Evaluation.SamplesEvaluator{TChromosome,TNet}"/>'s
/// live call path and R3 forbids a silent discard there. Throws from the string indexer because
/// Pooshit.Ai always resolves neurons by index.
/// </summary>
class FakeNet : INeuronalNet<FakeChromosome> {
    readonly Dictionary<int, float> values = new();

    public FakeNet(FakeChromosome chromosome) { }

    public List<float[]> InputValues { get; } = new();

    public List<FakeChromosome> Updates { get; } = new();

    public float this[string name] {
        get => throw new NotSupportedException($"{nameof(FakeNet)} resolves neurons by index; string lookup for '{name}' was not expected");
        set => throw new NotSupportedException($"{nameof(FakeNet)} resolves neurons by index; string lookup for '{name}' was not expected");
    }


    public float this[int index] {
        get => values.GetValueOrDefault(index);
        set => values[index] = value;
    }


    public void Compute() { }


    public void SetInputValues(float[] inputValues) => InputValues.Add(inputValues);


    public void Update(FakeChromosome configuration) => Updates.Add(configuration);
}
