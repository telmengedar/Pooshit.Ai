using System.Text.Json;

namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// the measurement lane. Reproduces the fixed problem/seed matrix, asserts the two invariants
/// that can only become more true as defects are fixed, and reports quality against the
/// committed baseline for a human to read - the benchmark asserting almost nothing about quality
/// is the load-bearing decision of design #9072 §11.3, not timidity
/// </summary>
[TestFixture, Parallelizable]
public class BenchmarkComparisonTests {

    [Test, Parallelizable, Explicit, Category("Benchmark")]
    [Description("Runs every (problem, seed) pair once, asserts I1 (every final fitness is finite and non-negative), I2 (every run's final fitness is at most its own generation-0 best, which elitism guarantees by construction at SampleCount = 0) and I3 (no generation in any run reports a non-finite fitness), then prints the per-seed table against Benchmarks/baseline.json. Quality itself is reported, never asserted - I3 is not quality: #9083 classifies a non-finite value as outright wrong rather than merely imprecise, the same bucket I1 already asserts against, so unlike a fitness delta it is asserted rather than only reported (DiVoid #9511).")]
    public void Benchmark_AgainstCommittedBaseline_HoldsInvariantsAndReportsComparison() {
        BenchmarkRunResult[] results = BenchmarkHarness.Run();

        Assert.Multiple(() => {
            foreach (BenchmarkRunResult result in results) {
                Assert.That(float.IsFinite(result.FinalFitness), Is.True,
                            $"I1 (finite): {result.ProblemName} seed {result.Seed} fitness={result.FinalFitness}");
                Assert.That(result.FinalFitness, Is.GreaterThanOrEqualTo(0.0f),
                            $"I1 (non-negative): {result.ProblemName} seed {result.Seed} fitness={result.FinalFitness}");
                Assert.That(result.FinalFitness, Is.LessThanOrEqualTo(result.GenerationZeroBest),
                            $"I2: {result.ProblemName} seed {result.Seed} final={result.FinalFitness} generationZeroBest={result.GenerationZeroBest}");
                Assert.That(result.NonFiniteGenerations, Is.Zero,
                            $"I3: {result.ProblemName} seed {result.Seed} reported {result.NonFiniteGenerations} generation(s) with a non-finite fitness - DiVoid #9511");
            }
        });

        BenchmarkReport.Print(LoadBaseline(), results);
    }

    static Baseline LoadBaseline() {
        string path = BenchmarkBaselineFile.Locate();
        if (!File.Exists(path))
            return new Baseline { Note = "(no baseline recorded yet)" };

        return JsonSerializer.Deserialize<Baseline>(File.ReadAllText(path))
               ?? throw new InvalidOperationException($"'{path}' deserialized to null - the committed baseline is empty or corrupt.");
    }
}
