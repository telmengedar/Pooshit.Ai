using System.Globalization;

namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// formats the benchmark comparison table printed by <see cref="BenchmarkComparisonTests"/>.
/// Quality is reported here, never asserted (design #9072 §11.3) - printing is the only place
/// this benchmark says anything about whether training got better or worse
/// </summary>
public static class BenchmarkReport {

    const string PairingCaveat =
        "NOTE: per-seed pairing below assumes the Rng implementation is unchanged since the baseline was recorded. " +
        "If DiVoid #9038 has landed since baseline.RecordedAt/Commit, the same seed no longer reproduces the same " +
        "stream and per-seed pairing is void - only the distribution summary remains meaningful.";

    /// <summary>
    /// prints the baseline header, the pairing caveat, and one section per problem: a per-seed
    /// paired table, a distribution summary, and a paired verdict count. No verdict word is
    /// printed for the suite as a whole (design #9072 §11.3)
    /// </summary>
    public static void Print(Baseline baseline, BenchmarkRunResult[] results) {
        Console.WriteLine("=== Benchmark comparison ===");
        Console.WriteLine($"Baseline recorded {baseline.RecordedAt:O} at commit {baseline.Commit} - \"{baseline.Note}\"");
        Console.WriteLine(PairingCaveat);
        if (baseline.Results.Any(r => r.NonFiniteGenerations == null))
            Console.WriteLine("NOTE: baseline predates non-finite-generation tracking (DiVoid #9511) - per-seed non-finite comparison below is unavailable where marked; only the current run's coverage line is meaningful there.");
        Console.WriteLine();

        foreach (IGrouping<string, BenchmarkRunResult> group in results.GroupBy(r => r.ProblemName)) {
            BenchmarkRunResult[] problemResults = group.OrderBy(r => r.Seed).ToArray();
            BenchmarkProblem problem = BenchmarkProblems.All.First(p => p.Name == group.Key);
            PrintProblem(problem, problemResults, baseline);
        }

        string[] presentNames = results.Select(r => r.ProblemName).Distinct().ToArray();
        foreach (string staleName in baseline.Results.Select(r => r.ProblemName).Distinct().Except(presentNames))
            Console.WriteLine($"NOTE: '{staleName}' is present in baseline.json but absent from the current harness (renamed or removed problem).");
    }

    static void PrintProblem(BenchmarkProblem problem, BenchmarkRunResult[] problemResults, Baseline baseline) {
        Console.WriteLine($"--- {problem.Name} ---");
        Console.WriteLine($"{"seed",-8}{"baseline",-16}{"current",-16}{"delta",-16}{"non-finite",-10}");

        int improved = 0;
        int regressed = 0;
        int unchanged = 0;
        int missingBaseline = 0;

        foreach (BenchmarkRunResult result in problemResults) {
            BaselineRunRecord baselineRecord = baseline.Results.FirstOrDefault(r => r.ProblemName == result.ProblemName && r.Seed == result.Seed);
            if (baselineRecord == null) {
                Console.WriteLine($"{result.Seed,-8}{"(absent)",-16}{Format(result.FinalFitness),-16}{"",-16}{FormatNonFinite(result.NonFiniteGenerations, null)}");
                missingBaseline++;
                continue;
            }

            float delta = result.FinalFitness - baselineRecord.FinalFitness;
            Console.WriteLine($"{result.Seed,-8}{Format(baselineRecord.FinalFitness),-16}{Format(result.FinalFitness),-16}{FormatSigned(delta),-16}{FormatNonFinite(result.NonFiniteGenerations, baselineRecord.NonFiniteGenerations)}");

            if (delta < 0.0f) improved++;
            else if (delta > 0.0f) regressed++;
            else unchanged++;
        }

        float[] sortedFitness = problemResults.Select(r => r.FinalFitness).OrderBy(f => f).ToArray();
        float[] sortedGenerations = problemResults.Select(r => (float)r.Generations).OrderBy(g => g).ToArray();
        int solved = problemResults.Count(r => r.FinalFitness <= problem.TargetFitness);

        Console.WriteLine($"distribution  median={Format(Median(sortedFitness))} min={Format(sortedFitness.First())} max={Format(sortedFitness.Last())} " +
                           $"solved={solved}/{problemResults.Length} (target {Format(problem.TargetFitness)}) medianGenerations={Median(sortedGenerations):F0}");

        string missingSuffix = missingBaseline > 0 ? $", {missingBaseline} absent from baseline" : "";
        Console.WriteLine($"paired verdict: improved on {improved}/{problemResults.Length} seeds, regressed on {regressed}/{problemResults.Length}, unchanged on {unchanged}/{problemResults.Length}{missingSuffix}");

        int affectedSeeds = problemResults.Count(r => r.NonFiniteGenerations > 0);
        int totalGenerations = problemResults.Sum(r => r.Generations);
        int totalNonFiniteGenerations = problemResults.Sum(r => r.NonFiniteGenerations);
        Console.WriteLine($"non-finite coverage: {affectedSeeds}/{problemResults.Length} seeds observed a non-finite fitness " +
                           $"({totalNonFiniteGenerations} of {totalGenerations} total generations affected) - I3 asserts this stays zero (DiVoid #9511)");
        Console.WriteLine();
    }

    /// <summary>
    /// per-seed non-finite suffix - silent for the boring case (nothing observed, nothing changed
    /// from baseline) so the common-case table stays exactly as compact as before this field
    /// existed; the moment there is anything to see, it prints. A count moving away from baseline
    /// is a stronger signal than a fitness delta (design #9511) and is marked accordingly.
    /// Internal and directly tested, mirroring <see cref="Median"/> - it is the only thing standing
    /// between a report print and a silently swallowed non-finite regression
    /// </summary>
    internal static string FormatNonFinite(int current, int? baselineValue) {
        if (baselineValue == null)
            return current > 0 ? $"  non-finite: {current} generation(s) (baseline: not recorded)" : "";
        if (current != baselineValue.Value)
            return $"  non-finite: {current} generation(s) (baseline: {baselineValue.Value}) !!";
        return current > 0 ? $"  non-finite: {current} generation(s) (unchanged)" : "";
    }

    /// <summary>
    /// median of an already-sorted array, or <see cref="float.NaN"/> for an empty array. Internal
    /// and directly tested (QA #9388 W4) - it is the only pure logic standing between a report
    /// print and a silently wrong distribution number
    /// </summary>
    internal static float Median(float[] sortedValues) {
        if (sortedValues.Length == 0)
            return float.NaN;
        int mid = sortedValues.Length / 2;
        return sortedValues.Length % 2 == 0 ? (sortedValues[mid - 1] + sortedValues[mid]) / 2.0f : sortedValues[mid];
    }

    static string Format(float value) => value.ToString("G6", CultureInfo.InvariantCulture);

    static string FormatSigned(float value) => (value >= 0.0f ? "+" : "") + Format(value);
}
