using Pooshit.Ai.Extensions;
using Pooshit.Ai.Net.Operations;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class NMathComputeTests {

    [Test, Parallelizable]
    public void Compute_EveryOperationType_ProducesPairwiseDistinctResults() {
        float[] results = Enum.GetValues<OperationType>()
                               .Select(op => NMath.Compute(6.0f, 2.0f, op))
                               .ToArray();

        Assert.That(results, Is.Unique);
    }


    [Test, Parallelizable]
    [TestCase(6.0f, 2.0f, 12.0f)]
    [TestCase(3.0f, 5.0f, 15.0f)]
    public void Compute_Multiply_ReturnsProduct(float lhs, float rhs, float expected) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Multiply), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(6.0f, 2.0f, 8.0f)]
    [TestCase(1.0f, 2.0f, 3.0f)]
    public void Compute_Add_ReturnsSum(float lhs, float rhs, float expected) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Add), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(6.0f, 2.0f, 4.0f)]
    [TestCase(1.0f, 5.0f, -4.0f)]
    public void Compute_Sub_ReturnsDifference(float lhs, float rhs, float expected) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Sub), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(6.0f, 2.0f, 3.0f)]
    [TestCase(10.0f, 2.0f, 5.0f)]
    public void Compute_Div_ReturnsQuotient(float lhs, float rhs, float expected) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Div), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(6.0f, 0.0f)]
    [TestCase(-6.0f, 0.0f)]
    [TestCase(0.0f, 0.0f)]
    public void Compute_Div_NonFiniteResult_GuardsToZero(float lhs, float rhs) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Div), Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    [TestCase(float.MaxValue, float.MaxValue)]
    [TestCase(float.MinValue, float.MinValue)]
    public void Compute_Add_NonFiniteResult_GuardsToZero(float lhs, float rhs) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Add), Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    [TestCase(float.MinValue, float.MaxValue)]
    [TestCase(float.MaxValue, float.MinValue)]
    public void Compute_Sub_NonFiniteResult_GuardsToZero(float lhs, float rhs) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Sub), Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    [TestCase(float.MaxValue, 2.0f)]
    [TestCase(float.MinValue, 2.0f)]
    public void Compute_Multiply_NonFiniteResult_GuardsToZero(float lhs, float rhs) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Multiply), Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    [TestCase(6.0f, 2.0f, 2.0f)]
    [TestCase(1.0f, 9.0f, 1.0f)]
    public void Compute_Min_ReturnsSmaller(float lhs, float rhs, float expected) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Min), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(6.0f, 2.0f, 6.0f)]
    [TestCase(1.0f, 9.0f, 9.0f)]
    public void Compute_Max_ReturnsLarger(float lhs, float rhs, float expected) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Max), Is.EqualTo(expected));
    }


    [Test, Parallelizable]
    [TestCase(6.0f, 2.0f)]
    [TestCase(3.0f, 3.0f)]
    public void Compute_Pow_ApproximatesTruePowerWithinTolerance(float lhs, float rhs) {
        double expected = Math.Pow(lhs, Math.Abs(rhs));
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Pow), Is.EqualTo(expected).Within(15).Percent);
    }


    [Test, Parallelizable]
    [TestCase(2.0f, 6.0f)]
    [TestCase(3.0f, 3.0f)]
    public void Compute_InvPow_ApproximatesTruePowerWithinTolerance(float lhs, float rhs) {
        double expected = Math.Pow(rhs, Math.Abs(lhs));
        Assert.That(NMath.Compute(lhs, rhs, OperationType.InvPow), Is.EqualTo(expected).Within(15).Percent);
    }


    [Test, Parallelizable]
    [TestCase(-6.0f, 2.0f)]
    [TestCase(0.0f, 2.0f)]
    public void Compute_Pow_BaseOutOfDomain_GuardsToZero(float lhs, float rhs) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.Pow), Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    [TestCase(2.0f, -6.0f)]
    [TestCase(2.0f, 0.0f)]
    public void Compute_InvPow_BaseOutOfDomain_GuardsToZero(float lhs, float rhs) {
        Assert.That(NMath.Compute(lhs, rhs, OperationType.InvPow), Is.EqualTo(0.0f));
    }
}
