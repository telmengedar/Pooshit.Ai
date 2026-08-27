using System.Diagnostics;
using System.Text.Json;

namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// overwrites the committed <c>Benchmarks/baseline.json</c> with a fresh run of the benchmark
/// harness. Run deliberately after a change that legitimately moves training quality, then commit
/// the rewritten file with the PR that caused the move, so the reviewer reads a diff of numbers
/// with a stated cause instead of triaging a red build (design #9072 §11.3 baseline lifecycle)
/// </summary>
[TestFixture, Parallelizable]
public class RecordBaselineTests {

    /// <summary>
    /// why this recording was taken. Update this before re-recording, so the PR diff explains
    /// which change moved the baseline
    /// </summary>
    const string Note = "DiVoid #9511: continues recording NonFiniteGenerations (e.g. FeedForward.MultiplyMinus seed 7 = 1) - schema addition only. FinalFitness values are unchanged from master's own #9043 recording (832711f); this branch does not move them";

    /// <summary>
    /// the only thing that actually authorises overwriting the committed baseline. [Explicit] alone
    /// is not a guard against this test running - a name filter such as
    /// <c>--filter "FullyQualifiedName~Benchmark"</c> selects and runs [Explicit] tests too, and
    /// would otherwise silently overwrite the artefact the benchmark measures against (QA #9388 W1)
    /// </summary>
    const string RecordBaselineEnvironmentVariable = "POOSHIT_RECORD_BASELINE";

    [Test, Parallelizable, Explicit]
    [Description("Overwrites Benchmarks/baseline.json, guarded by [Explicit] plus the POOSHIT_RECORD_BASELINE=1 environment variable so a name filter alone cannot trigger it.")]
    public void RecordBaseline_Run_OverwritesCommittedBaselineFile() {
        if (Environment.GetEnvironmentVariable(RecordBaselineEnvironmentVariable) != "1")
            Assert.Ignore($"Set {RecordBaselineEnvironmentVariable}=1 to actually overwrite the committed baseline. " +
                           "[Explicit] alone does not guard this - a name filter such as --filter \"FullyQualifiedName~Benchmark\" can still select and run it.");

        BenchmarkRunResult[] results = BenchmarkHarness.Run();

        Baseline baseline = new() {
            RecordedAt = DateTime.UtcNow,
            Commit = CurrentCommit(),
            Note = Note,
            Results = results.Select(r => new BaselineRunRecord(r.ProblemName, r.Seed, r.FinalFitness, r.Generations, r.NonFiniteGenerations)).ToArray()
        };

        string path = BenchmarkBaselineFile.Locate();
        File.WriteAllText(path, JsonSerializer.Serialize(baseline, new JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine($"Baseline written to {path}");
    }

    static string CurrentCommit() {
        try {
            ProcessStartInfo startInfo = new("git", "rev-parse --short HEAD") {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using Process process = Process.Start(startInfo);
            string output = process?.StandardOutput.ReadToEnd().Trim();
            process?.WaitForExit();
            return string.IsNullOrEmpty(output) ? "unknown" : output;
        }
        catch (Exception) {
            return "unknown";
        }
    }
}
