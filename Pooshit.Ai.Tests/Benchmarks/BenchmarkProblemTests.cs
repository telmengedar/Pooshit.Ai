using Pooshit.Ai.Net.DynamicBO;

namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// pins the early-exit generation count in <see cref="BenchmarkProblem{TChromosome,TNet}.Run"/> -
/// QA #9388 W4 found the <c>+1</c> correction branch genuinely unexercised (all 24 recorded pairs
/// ran the full configured <c>Runs</c>), yet it is the only thing standing between a correct
/// <c>Generations</c> count and a silent off-by-one
/// </summary>
[TestFixture, Parallelizable]
public class BenchmarkProblemTests {

    [Test, Parallelizable]
    [Description("Population.Train's AfterRun callback does not fire for the generation that trips TargetFitness - it breaks before invoking it - so Generations must be corrected by one on early exit or it silently undercounts. A target of float.MaxValue is satisfied by the very first generation regardless of seed, forcing that branch: without the +1 correction this would report 0.")]
    public void Run_TargetFitnessTriviallySatisfiedByFirstGeneration_ReportsOneGenerationExecuted() {
        BenchmarkProblem<DynamicBOConfiguration, DynamicBONet> problem = new(
            "Test.TrivialEarlyExit",
            populationSize: 2,
            generator: rng => new(["x"], ["y"], rng),
            samples: () => [new(new { x = 1 }, new { y = 1 })],
            runs: 10,
            rivalism: 1,
            targetFitness: float.MaxValue);

        BenchmarkRunResult result = problem.Run(seed: 1);

        Assert.That(result.Generations, Is.EqualTo(1));
    }

    [Test, Parallelizable]
    [Description("An unreachable target (float.Epsilon, the EvolutionSetup default) exhausts the configured Runs without ever tripping early exit, so Generations must equal Runs exactly rather than Runs - 1 or Runs + 1.")]
    public void Run_TargetFitnessUnreachable_ReportsGenerationsEqualToRuns() {
        BenchmarkProblem<DynamicBOConfiguration, DynamicBONet> problem = new(
            "Test.RunsExhausted",
            populationSize: 2,
            generator: rng => new(["x"], ["y"], rng),
            samples: () => [new(new { x = 1 }, new { y = 1 })],
            runs: 3,
            rivalism: 1,
            targetFitness: float.Epsilon);

        BenchmarkRunResult result = problem.Run(seed: 1);

        Assert.That(result.Generations, Is.EqualTo(3));
    }
}
