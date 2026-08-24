using Pooshit.Ai.Net;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// a constant-zero oracle net on purpose (R3, test README): output queries always resolve to
/// whatever was last written to that index, defaulting to 0 for anything never written. The
/// design's genetics-mechanics phase (#9072 §9.2) inverted the whole problem so it needs zero
/// nets - StubFitnessEvaluator scores chromosomes directly - so FakeNet is never actually
/// instantiated by any Lane 1b test; only <see cref="SamplesEvaluatorTests"/> exercises it, and
/// only through this[int]/SetInputValues/Compute/Update, never by reading a recorded call log.
/// A recording surface (input arrays, index writes, update calls) was carried here from an
/// earlier phase in anticipation of exactly that consumer; since it never appeared it was
/// removed - the unconsumed-double condition (design #9072 §16 addendum).
/// </summary>
class FakeNet : INeuronalNet<FakeChromosome> {
    readonly Dictionary<int, float> values = new();

    public FakeNet(FakeChromosome chromosome) { }

    public float this[string name] {
        get => throw new NotSupportedException($"{nameof(FakeNet)} resolves neurons by index; string lookup for '{name}' was not expected");
        set => throw new NotSupportedException($"{nameof(FakeNet)} resolves neurons by index; string lookup for '{name}' was not expected");
    }


    public float this[int index] {
        get => values.GetValueOrDefault(index);
        set => values[index] = value;
    }


    public void Compute() { }


    public void SetInputValues(float[] inputValues) { }


    public void Update(FakeChromosome configuration) { }
}
