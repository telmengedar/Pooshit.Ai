using Pooshit.Ai.Extensions;
using Pooshit.Ai.Net.Operations;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class NMathActivationTests {
    static float[] Probes() => [-10.0f, -0.3f, 0.3f, 10.0f];

    [Test, Parallelizable]
    public void Activation_EveryFunc_ProducesPairwiseDistinctResponseVectors() {
        List<float[]> vectors = Enum.GetValues<ActivationFunc>()
                                     .Select(func => Probes().Select(p => p.Activation(func)).ToArray())
                                     .ToList();

        for (int lhs = 0; lhs < vectors.Count; ++lhs)
        for (int rhs = lhs + 1; rhs < vectors.Count; ++rhs)
            Assert.That(vectors[lhs], Is.Not.EqualTo(vectors[rhs]));
    }


    [Test, Parallelizable]
    public void Activation_EveryFunc_NeverProducesNonFiniteResult() {
        foreach (ActivationFunc func in Enum.GetValues<ActivationFunc>())
        foreach (float probe in Probes())
            Assert.That(float.IsFinite(probe.Activation(func)), Is.True);
    }


    [Test, Parallelizable]
    [TestCase(5.0f, 5.0f)]
    [TestCase(-5.0f, -5.0f)]
    public void Activation_None_ReturnsValueUnchanged(float value, float expected) {
        Assert.That(value.Activation(ActivationFunc.None), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(-1.0f, 0.0f)]
    [TestCase(0.0f, 1.0f)]
    [TestCase(1.0f, 1.0f)]
    public void Activation_BinaryStep_ReturnsZeroOrOne(float value, float expected) {
        Assert.That(value.Activation(ActivationFunc.BinaryStep), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(0.0f, 0.5)]
    [TestCase(-10.0f, 0.045454545454545456)]
    public void Activation_Sigmoid_ApproximatesClosedFormWithinTolerance(float value, double expected) {
        Assert.That(value.Activation(ActivationFunc.Sigmoid), Is.EqualTo(expected).Within(0.0001));
    }


    [Test, Parallelizable]
    [TestCase(2.0f)]
    [TestCase(-1.0f)]
    public void Activation_Sin_ApproximatesTrueSineWithinTolerance(float value) {
        Assert.That(value.Activation(ActivationFunc.Sin), Is.EqualTo(Math.Sin(value)).Within(0.0001));
    }


    [Test, Parallelizable]
    [TestCase(2.0f)]
    [TestCase(-1.0f)]
    public void Activation_Tanh_ApproximatesTrueTanhWithinTolerance(float value) {
        Assert.That(value.Activation(ActivationFunc.Tanh), Is.EqualTo(Math.Tanh(value)).Within(0.0001));
    }


    [Test, Parallelizable]
    [TestCase(-5.0f, 0.0f)]
    [TestCase(5.0f, 5.0f)]
    public void Activation_ReLU_ClampsNegativesToZero(float value, float expected) {
        Assert.That(value.Activation(ActivationFunc.ReLU), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(-10.0f, -1.0f)]
    [TestCase(10.0f, 10.0f)]
    public void Activation_LeakyReLU_DampensNegativesInsteadOfClamping(float value, float expected) {
        Assert.That(value.Activation(ActivationFunc.LeakyReLU), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(4.0f, 0.25f)]
    [TestCase(-2.0f, -0.5f)]
    public void Activation_Reciprocal_ReturnsOneOverValue(float value, float expected) {
        Assert.That(value.Activation(ActivationFunc.Reciprocal), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    public void Activation_Reciprocal_AtZero_GuardsNonFiniteResultToZero() {
        Assert.That(0.0f.Activation(ActivationFunc.Reciprocal), Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    [TestCase(0.0f, 0.0)]
    [TestCase(2.0f, 1.6666666666666667)]
    public void Activation_Swish_ApproximatesValueTimesClosedFormSigmoidWithinTolerance(float value, double expected) {
        Assert.That(value.Activation(ActivationFunc.Swish), Is.EqualTo(expected).Within(0.0001));
    }


    [Test, Parallelizable]
    [TestCase(4.0f, 2.0)]
    [TestCase(9.0f, 3.0)]
    public void Activation_Sqrt_ApproximatesTrueSquareRootWithinTolerance(float value, double expected) {
        Assert.That(value.Activation(ActivationFunc.Sqrt), Is.EqualTo(expected).Within(1).Percent);
    }


    [Test, Parallelizable]
    [TestCase(4.0f, 16.0f)]
    [TestCase(-3.0f, 9.0f)]
    public void Activation_Pow2_ReturnsSquare(float value, float expected) {
        Assert.That(value.Activation(ActivationFunc.Pow2), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(2.7f, 2.0f)]
    [TestCase(-2.7f, -3.0f)]
    public void Activation_Floor_RoundsDown(float value, float expected) {
        Assert.That(value.Activation(ActivationFunc.Floor), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(2.3f, 3.0f)]
    [TestCase(-2.3f, -2.0f)]
    public void Activation_Ceiling_RoundsUp(float value, float expected) {
        Assert.That(value.Activation(ActivationFunc.Ceiling), Is.EqualTo(expected));
    }
}
