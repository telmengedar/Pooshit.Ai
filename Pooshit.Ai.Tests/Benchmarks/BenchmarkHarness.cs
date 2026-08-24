namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// executes the fixed benchmark problem/seed matrix. Every individual run is single-threaded,
/// which is what reproducibility requires; the harness parallelises across (problem, seed) pairs
/// instead - the concurrency axis the benchmark actually needs (design #9072 §11.2). Results are
/// sorted into a fixed (ProblemName, Seed) order before being returned, so that re-recording an
/// unchanged run produces an empty diff against the committed baseline rather than a full rewrite
/// caused only by <c>.AsParallel()</c> completion order (QA #9388 W2)
/// </summary>
public static class BenchmarkHarness {

    /// <summary>
    /// the eight seeds every problem is measured against (confirmed by Toni, design #9072 §15 Q2).
    /// Never zero - <see cref="Pooshit.Ai.Extern.Rng"/> treats seed 0 as a clock-seeded fallback
    /// (design #9072 §10.2 H10)
    /// </summary>
    public static readonly long[] Seeds = [1, 2, 3, 4, 5, 6, 7, 8];

    /// <summary>
    /// runs every problem in <see cref="BenchmarkProblems.All"/> against every seed in
    /// <see cref="Seeds"/> and returns one result per pair, ordered by problem name then seed
    /// </summary>
    public static BenchmarkRunResult[] Run() {
        return BenchmarkProblems.All
                                 .SelectMany(problem => Seeds.Select(seed => (problem, seed)))
                                 .AsParallel()
                                 .Select(pair => pair.problem.Run(pair.seed))
                                 .ToArray()
                                 .OrderBy(r => r.ProblemName, StringComparer.Ordinal)
                                 .ThenBy(r => r.Seed)
                                 .ToArray();
    }
}
