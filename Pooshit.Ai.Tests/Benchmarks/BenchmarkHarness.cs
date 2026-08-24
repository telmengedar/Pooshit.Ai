namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// executes the fixed benchmark problem/seed matrix. Every individual run is single-threaded,
/// which is what reproducibility requires; the harness parallelises across (problem, seed) pairs
/// instead - the concurrency axis the benchmark actually needs (design #9072 §11.2)
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
    /// <see cref="Seeds"/> and returns one result per pair
    /// </summary>
    public static BenchmarkRunResult[] Run() {
        return BenchmarkProblems.All
                                 .SelectMany(problem => Seeds.Select(seed => (problem, seed)))
                                 .AsParallel()
                                 .Select(pair => pair.problem.Run(pair.seed))
                                 .ToArray();
    }
}
