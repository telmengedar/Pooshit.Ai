namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// the last recorded benchmark measurement, committed to the repository as
/// <c>Benchmarks/baseline.json</c>. A record with provenance, not an expectation (design #9072
/// §5.3) - <see cref="BenchmarkComparisonTests"/> reports against it but never asserts that
/// quality did not move
/// </summary>
public sealed class Baseline {

    /// <summary>
    /// when this baseline was recorded
    /// </summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// git commit the recording was taken at
    /// </summary>
    public string Commit { get; set; }

    /// <summary>
    /// why this recording was taken - the string a reviewer reads instead of triaging a red build
    /// when the baseline moves
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// one entry per (problem, seed) pair measured at recording time
    /// </summary>
    public BaselineRunRecord[] Results { get; set; } = [];
}
