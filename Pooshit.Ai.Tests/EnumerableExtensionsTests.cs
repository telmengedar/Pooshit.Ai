using Pooshit.Ai.Extensions;
using Pooshit.Ai.Extern;

namespace NightlyCode.Ai.Tests;

class SequenceRng : IRng {
    readonly int[] values;
    int index;

    public SequenceRng(params int[] values) => this.values = values;

    public long NextLong() => throw new NotSupportedException();
    public int NextInt() => throw new NotSupportedException();
    public int NextInt(int max) => values[index++];
    public float NextFloat() => throw new NotSupportedException();
    public float NextFloatRange() => throw new NotSupportedException();
    public double NextDouble() => throw new NotSupportedException();
}

[TestFixture, Parallelizable]
public class EnumerableExtensionsTests {

    [Test, Parallelizable]
    public void Shuffle_ArraySource_DoesNotMutateSourceArray() {
        int[] source = [1, 2, 3, 4, 5];
        Rng rng = new(1);

        source.Shuffle(rng).ToArray();

        Assert.That(source, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test, Parallelizable]
    public void Shuffle_ArraySource_ResultIsPermutationOfSource() {
        int[] source = [1, 2, 3, 4, 5];
        Rng rng = new(1);

        int[] shuffled = source.Shuffle(rng).ToArray();

        Assert.That(shuffled.OrderBy(v => v), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test, Parallelizable]
    public void Shuffle_KnownRngSequence_ProducesExpectedPermutation() {
        int[] source = [10, 20, 30, 40, 50];
        SequenceRng rng = new(2, 3, 0, 1, 0);

        int[] shuffled = source.Shuffle(rng).ToArray();

        Assert.That(shuffled, Is.EqualTo(new[] { 30, 40, 10, 20, 50 }));
    }

    [Test, Parallelizable]
    public void Shuffle_ArraySource_TakeYieldsDistinctElements() {
        int[] source = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        Rng rng = new(1);

        int[] sample = source.Shuffle(rng).Take(4).ToArray();

        Assert.That(sample.Distinct().Count(), Is.EqualTo(4));
        Assert.That(sample, Is.SubsetOf(source));
    }

    [Test, Parallelizable]
    public void RandomSample_ArraySource_DoesNotMutateSourceArray() {
        int[] source = [1, 2, 3, 4, 5];
        Rng rng = new(1);

        source.RandomSample(rng, 3);

        Assert.That(source, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test, Parallelizable]
    public void RandomSample_ArraySource_ResultIsDistinctSubsetOfSource() {
        int[] source = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        Rng rng = new(1);

        int[] sample = source.RandomSample(rng, 4);

        Assert.That(sample.Length, Is.EqualTo(4));
        Assert.That(sample.Distinct().Count(), Is.EqualTo(4));
        Assert.That(sample, Is.SubsetOf(source));
    }

    [Test, Parallelizable]
    public void RandomSample_KnownRngSequence_ProducesExpectedElements() {
        int[] source = [10, 20, 30, 40, 50];
        SequenceRng rng = new(2, 3, 0);

        int[] sample = source.RandomSample(rng, 3);

        Assert.That(sample, Is.EqualTo(new[] { 30, 40, 10 }));
    }

    [Test, Parallelizable]
    public void RandomSample_KnownRngSequenceRevisitingDisplacedIndex_ProducesExpectedElements() {
        int[] source = [10, 20, 30, 40, 50];
        SequenceRng rng = new(0, 0);

        int[] sample = source.RandomSample(rng, 2);

        Assert.That(sample, Is.EqualTo(new[] { 10, 50 }));
    }

    [Test, Parallelizable]
    public void RandomSample_CountGreaterThanSourceLength_ClampsToSourceLength() {
        int[] source = [1, 2, 3];
        Rng rng = new(1);

        int[] sample = source.RandomSample(rng, 10);

        Assert.That(sample.Length, Is.EqualTo(3));
        Assert.That(sample.OrderBy(v => v), Is.EqualTo(new[] { 1, 2, 3 }));
    }
}
