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
    const string Note = "first recording";

    [Test, Parallelizable, Explicit]
    [Description("Runs the benchmark harness and overwrites Benchmarks/baseline.json in the source tree, located by walking up from the test binary to the .csproj rather than writing a build-output copy. Never part of the default or benchmark-comparison lane.")]
    public void RecordBaseline_Run_OverwritesCommittedBaselineFile() {
        BenchmarkRunResult[] results = BenchmarkHarness.Run();

        Baseline baseline = new() {
            RecordedAt = DateTime.UtcNow,
            Commit = CurrentCommit(),
            Note = Note,
            Results = results.Select(r => new BaselineRunRecord(r.ProblemName, r.Seed, r.FinalFitness, r.Generations)).ToArray()
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
