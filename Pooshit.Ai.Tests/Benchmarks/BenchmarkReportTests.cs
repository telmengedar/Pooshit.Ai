namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// pins <see cref="BenchmarkReport.Median"/> - one of the pure functions QA #9388 W4 flagged as
/// untested despite being the only thing standing between a report print and a silently wrong
/// distribution number
/// </summary>
[TestFixture, Parallelizable]
public class BenchmarkReportTests {

    [Test, Parallelizable]
    [Description("An odd-length sorted array's median is its exact middle element.")]
    public void Median_OddLengthArray_ReturnsMiddleElement() {
        Assert.That(BenchmarkReport.Median([1.0f, 2.0f, 3.0f]), Is.EqualTo(2.0f));
    }

    [Test, Parallelizable]
    [Description("An even-length sorted array's median is the average of its two middle elements.")]
    public void Median_EvenLengthArray_ReturnsAverageOfTwoMiddleElements() {
        Assert.That(BenchmarkReport.Median([1.0f, 2.0f, 3.0f, 4.0f]), Is.EqualTo(2.5f));
    }

    [Test, Parallelizable]
    [Description("A single-element array's median is that element - the boundary case between the odd and even branches.")]
    public void Median_SingleElementArray_ReturnsThatElement() {
        Assert.That(BenchmarkReport.Median([7.0f]), Is.EqualTo(7.0f));
    }

    [Test, Parallelizable]
    [Description("An empty array has no median; NaN signals 'not computable' rather than a misleading zero or an exception.")]
    public void Median_EmptyArray_ReturnsNaN() {
        Assert.That(BenchmarkReport.Median([]), Is.NaN);
    }
}
