using Pooshit.Ai.Extensions;
using Pooshit.Ai.Net.Operations;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class NMathAggregateTests {

    [Test, Parallelizable]
    public void Aggregate_EveryAggregateType_ProducesPairwiseDistinctResults() {
        float[] fixture = [1.0f, 2.0f, 3.0f, 4.0f, 10.0f];
        float[] results = Enum.GetValues<AggregateType>()
                               .Select(aggregate => fixture.Aggregate(aggregate))
                               .ToArray();

        Assert.That(results, Is.Unique);
    }


    [Test, Parallelizable]
    [TestCase(new[] { 1.0f, 2.0f, 3.0f, 4.0f, 10.0f }, 20.0f)]
    [TestCase(new[] { 1.0f, 1.0f, 1.0f }, 3.0f)]
    public void Aggregate_Sum_AddsEveryValue(float[] values, float expected) {
        Assert.That(values.Aggregate(AggregateType.Sum), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(new[] { 1.0f, 2.0f, 3.0f, 4.0f, 10.0f }, 4.0f)]
    [TestCase(new[] { 2.0f, 4.0f, 9.0f }, 5.0f)]
    public void Aggregate_Average_ReturnsMean(float[] values, float expected) {
        Assert.That(values.Aggregate(AggregateType.Average), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(new[] { 5.0f, 1.0f, 3.0f }, 3.0f)]
    [TestCase(new[] { 8.0f, 2.0f, 4.0f, 6.0f }, 6.0f)]
    public void Aggregate_Median_ReturnsUpperMiddleOfSortedValues(float[] values, float expected) {
        Assert.That(values.Aggregate(AggregateType.Median), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(new[] { 5.0f, 1.0f, 3.0f }, 1.0f)]
    [TestCase(new[] { 8.0f, 2.0f, 4.0f }, 2.0f)]
    public void Aggregate_Min_ReturnsSmallestValue(float[] values, float expected) {
        Assert.That(values.Aggregate(AggregateType.Min), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(new[] { 5.0f, 1.0f, 3.0f }, 5.0f)]
    [TestCase(new[] { 8.0f, 2.0f, 4.0f }, 8.0f)]
    public void Aggregate_Max_ReturnsLargestValue(float[] values, float expected) {
        Assert.That(values.Aggregate(AggregateType.Max), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(new[] { 1.0f, 2.0f, 3.0f, 4.0f, 10.0f }, 4.6f)]
    [TestCase(new[] { 1.0f, 1.0f, 7.0f }, 3.4f)]
    public void Aggregate_AverageToMax_WeightsMeanAndMax(float[] values, float expected) {
        Assert.That(values.Aggregate(AggregateType.AverageToMax), Is.EqualTo(expected).Within(0.0001f));
    }


    [Test, Parallelizable]
    public void Aggregate_SumOnEmptySequence_ReturnsZero() {
        Assert.That(Array.Empty<float>().Aggregate(AggregateType.Sum), Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    public void Aggregate_AverageOnEmptySequence_Throws() {
        Assert.That(() => Array.Empty<float>().Aggregate(AggregateType.Average), Throws.InvalidOperationException);
    }


    [Test, Parallelizable]
    public void Aggregate_MedianOnEmptySequence_Throws() {
        Assert.That(() => Array.Empty<float>().Aggregate(AggregateType.Median), Throws.InstanceOf<IndexOutOfRangeException>());
    }


    [Test, Parallelizable]
    public void Aggregate_MinOnEmptySequence_Throws() {
        Assert.That(() => Array.Empty<float>().Aggregate(AggregateType.Min), Throws.InvalidOperationException);
    }


    [Test, Parallelizable]
    public void Aggregate_MaxOnEmptySequence_Throws() {
        Assert.That(() => Array.Empty<float>().Aggregate(AggregateType.Max), Throws.InvalidOperationException);
    }


    [Test, Parallelizable]
    public void Aggregate_AverageToMaxOnEmptySequence_ReturnsZero() {
        Assert.That(Array.Empty<float>().Aggregate(AggregateType.AverageToMax), Is.EqualTo(0.0f));
    }
}
