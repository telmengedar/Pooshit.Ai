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
    [Description("Nothing observed and nothing changed from baseline (the common case for every run this problem set has ever produced) prints nothing - the per-seed table must stay exactly as compact as before DiVoid #9511's field existed, so a reviewer is not shown a wall of '0 (unchanged)' on every single row.")]
    public void FormatNonFinite_ZeroCurrentMatchingZeroBaseline_ReturnsEmpty() {
        Assert.That(BenchmarkReport.FormatNonFinite(0, 0), Is.Empty);
    }

    [Test, Parallelizable]
    [Description("A nonzero count that exactly matches baseline still prints - silence is reserved for the all-zero case, not for 'unchanged', because a persistently nonzero baseline is itself worth a reviewer's attention on every row it appears.")]
    public void FormatNonFinite_NonZeroCurrentMatchingNonZeroBaseline_ReportsUnchanged() {
        Assert.That(BenchmarkReport.FormatNonFinite(3, 3), Does.Contain("unchanged"));
    }

    [Test, Parallelizable]
    [Description("A count that differs from baseline - in either direction - is the strictly-stronger-than-fitness signal DiVoid #9511 calls out, and must be visually marked rather than blending into a routine delta row.")]
    public void FormatNonFinite_CurrentDiffersFromBaseline_IsMarked() {
        Assert.That(BenchmarkReport.FormatNonFinite(2, 0), Does.Contain("!!"));
    }

    [Test, Parallelizable]
    [Description("The differs-from-baseline check must fire in both directions - a count DROPPING from a nonzero baseline (e.g. a fix reducing persistence) is still worth flagging, not only a count rising. Kills a mutant that narrows the inequality to 'increased only'.")]
    public void FormatNonFinite_CurrentBelowBaseline_IsMarked() {
        Assert.That(BenchmarkReport.FormatNonFinite(0, 3), Does.Contain("!!"));
    }

    [Test, Parallelizable]
    [Description("A baseline recorded before DiVoid #9511 has no NonFiniteGenerations value at all (null, not 0). A current run of zero against that unmeasured baseline must stay silent - there is nothing to report - but the sibling case below proves null is still distinguished from an actual zero once there is something to say.")]
    public void FormatNonFinite_ZeroCurrentAgainstUnrecordedBaseline_ReturnsEmpty() {
        Assert.That(BenchmarkReport.FormatNonFinite(0, null), Is.Empty);
    }

    [Test, Parallelizable]
    [Description("A nonzero current count against a baseline that predates tracking (null) must say the baseline was never recorded, not silently claim the baseline was zero - the exact fabrication DiVoid #9511 warns against, now checked at the formatting boundary as well as at deserialization.")]
    public void FormatNonFinite_NonZeroCurrentAgainstUnrecordedBaseline_ReportsNotRecorded() {
        Assert.That(BenchmarkReport.FormatNonFinite(2, null), Does.Contain("not recorded"));
    }
}
