using Pooshit.Ai.Extensions;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class AMathTests {

    [Parallelizable]
    [TestCase(1.0f, 1.0f)]
    [TestCase(4.0f, 2.0f)]
    [TestCase(9.0f, 3.0f)]
    [TestCase(16.0f, 4.0f)]
    [TestCase(11.27f, 3.3570820663189036791182066449139f)]
    [TestCase(0.0001f, 0.01f)]
    public void TestInverseSquareRoot(float x, float y) {
        Assert.That(MathF.Abs(1.0f/x.InverseSquareRoot() - y), Is.LessThan(0.01f));
    }


    [Test, Parallelizable]
    public void InverseSquareRoot_LargeArgument_ApproximatesTrueValueWithinRelativeTolerance() {
        Assert.That(1.0f / 1e10f.InverseSquareRoot(), Is.EqualTo(100000.0).Within(1).Percent);
    }


    [Test, Parallelizable]
    public void InverseSquareRoot_AtZero_ReturnsLargeFiniteValue() {
        float result = 0.0f.InverseSquareRoot();
        Assert.That(result, Is.GreaterThan(1e15f));
        Assert.That(float.IsFinite(result), Is.True);
    }


    [Test, Parallelizable]
    [TestCase(-1.0f)]
    [TestCase(-4.0f)]
    [TestCase(-0.0001f, Description = "pre-fix this returned a finite -4.55e-37 (reciprocal -2.2e+36) - wrong sign, undetectable by any IsNaN/IsInfinity check")]
    public void InverseSquareRoot_NegativeArgument_ReturnsNaN(float value) {
        Assert.That(float.IsNaN(value.InverseSquareRoot()), Is.True);
    }


    [Test, Parallelizable]
    [TestCase(6.0, 2.0)]
    [TestCase(100.0, 0.5)]
    [TestCase(4.0, 0.5)]
    public void Power_PositiveBase_ApproximatesTruePowerWithinTolerance(double a, double b) {
        Assert.That(AMath.Power(a, b), Is.EqualTo(Math.Pow(a, b)).Within(15).Percent);
    }


    [Test, Parallelizable]
    public void Power_LargeExponent_ApproximatesTruePowerWithinWiderTolerance() {
        Assert.That(AMath.Power(2.0, 6.0), Is.EqualTo(Math.Pow(2.0, 6.0)).Within(35).Percent);
    }


    [Test, Parallelizable]
    [TestCase(-2.0)]
    [TestCase(-0.0001)]
    public void Power_NegativeBase_ReturnsNaN(double a) {
        Assert.That(double.IsNaN(AMath.Power(a, 2.0)), Is.True);
    }


    [Test, Parallelizable]
    public void Power_ZeroBase_ReturnsNaN() {
        Assert.That(double.IsNaN(AMath.Power(0.0, 2.0)), Is.True);
    }
}
