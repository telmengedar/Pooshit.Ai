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

    [Test, Parallelizable]
    [Description("Zero current matching zero baseline prints nothing, keeping the per-seed table as compact as before this field existed. DiVoid #9511.")]
    public void FormatNonFinite_ZeroCurrentMatchingZeroBaseline_ReturnsEmpty() {
        Assert.That(BenchmarkReport.FormatNonFinite(0, 0), Is.Empty);
    }

    [Test, Parallelizable]
    [Description("A nonzero count matching baseline still prints 'unchanged' rather than staying silent. DiVoid #9511.")]
    public void FormatNonFinite_NonZeroCurrentMatchingNonZeroBaseline_ReportsUnchanged() {
        Assert.That(BenchmarkReport.FormatNonFinite(3, 3), Does.Contain("unchanged"));
    }

    [Test, Parallelizable]
    [Description("A count differing from baseline is visually marked with '!!'. DiVoid #9511.")]
    public void FormatNonFinite_CurrentDiffersFromBaseline_IsMarked() {
        Assert.That(BenchmarkReport.FormatNonFinite(2, 0), Does.Contain("!!"));
    }

    [Test, Parallelizable]
    [Description("The differs-from-baseline mark fires when the count drops too, not only when it rises. DiVoid #9511.")]
    public void FormatNonFinite_CurrentBelowBaseline_IsMarked() {
        Assert.That(BenchmarkReport.FormatNonFinite(0, 3), Does.Contain("!!"));
    }

    [Test, Parallelizable]
    [Description("Zero current against an unrecorded (null) baseline prints nothing. DiVoid #9511.")]
    public void FormatNonFinite_ZeroCurrentAgainstUnrecordedBaseline_ReturnsEmpty() {
        Assert.That(BenchmarkReport.FormatNonFinite(0, null), Is.Empty);
    }

    [Test, Parallelizable]
    [Description("A nonzero current count against an unrecorded (null) baseline reports 'not recorded', never a fabricated zero. DiVoid #9511.")]
    public void FormatNonFinite_NonZeroCurrentAgainstUnrecordedBaseline_ReportsNotRecorded() {
        Assert.That(BenchmarkReport.FormatNonFinite(2, null), Does.Contain("not recorded"));
    }
}
