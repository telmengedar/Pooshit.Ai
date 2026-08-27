using Pooshit.Ai.Extensions;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class NMathFiniteOrZeroTests {

    [Test, Parallelizable]
    [TestCase(5.0f, 5.0f)]
    [TestCase(-5.0f, -5.0f)]
    [TestCase(0.0f, 0.0f)]
    [TestCase(float.MaxValue, float.MaxValue)]
    public void FiniteOrZero_FiniteValue_ReturnsValueUnchanged(float value, float expected) {
        Assert.That(value.FiniteOrZero(), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    public void FiniteOrZero_NaN_ReturnsZero() {
        Assert.That(float.NaN.FiniteOrZero(), Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    public void FiniteOrZero_PositiveInfinity_ReturnsZero() {
        Assert.That(float.PositiveInfinity.FiniteOrZero(), Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    public void FiniteOrZero_NegativeInfinity_ReturnsZero() {
        Assert.That(float.NegativeInfinity.FiniteOrZero(), Is.EqualTo(0.0f));
    }
}
