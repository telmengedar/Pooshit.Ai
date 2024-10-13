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
    public void TestInverseSquareRoot(float x, float y) {
        Assert.That(MathF.Abs(1.0f/x.InverseSquareRoot() - y), Is.LessThan(0.01f));
    }
}