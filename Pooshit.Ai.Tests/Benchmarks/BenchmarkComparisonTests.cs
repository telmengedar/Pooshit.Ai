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
    [Description("Asserts I1 (finite, non-negative fitness), I2 (final fitness never worse than generation-0 best) and I3 (no generation reports a non-finite fitness), then prints the per-seed comparison against Benchmarks/baseline.json. DiVoid #9511.")]
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
