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

    [Test, Parallelizable]
    [Description("A well-behaved sample never produces a non-finite fitness, so NonFiniteGenerations is zero - the sibling proving the field varies with input (R1). DiVoid #9511.")]
    public void Run_WellBehavedSamples_ReportsZeroNonFiniteGenerations() {
        BenchmarkProblem<DynamicBOConfiguration, DynamicBONet> problem = new(
            "Test.WellBehaved",
            populationSize: 2,
            generator: rng => new(["x"], ["y"], rng),
            samples: () => [new(new { x = 1 }, new { y = 1 })],
            runs: 3,
            rivalism: 1,
            targetFitness: float.Epsilon);

        BenchmarkRunResult result = problem.Run(seed: 1);

        Assert.That(result.NonFiniteGenerations, Is.Zero);
    }

    [Test, Parallelizable]
    [Description("An expected output of NaN forces every generation's fitness non-finite, proving Run() actually wires OnNonFiniteFitness through to NonFiniteGenerations end-to-end. DiVoid #9511.")]
    public void Run_ExpectedOutputIsNaN_ReportsNonFiniteGenerationsEqualToGenerationsExecuted() {
        BenchmarkProblem<DynamicBOConfiguration, DynamicBONet> problem = new(
            "Test.NonFiniteTarget",
            populationSize: 2,
            generator: rng => new(["x"], ["y"], rng),
            samples: () => [new(new { x = 1 }, new { y = float.NaN })],
            runs: 3,
            rivalism: 1,
            targetFitness: float.Epsilon);

        BenchmarkRunResult result = problem.Run(seed: 1);

        Assert.That(result.Generations, Is.EqualTo(3), "a NaN-based comparison against TargetFitness is never true, so Runs must be exhausted exactly like the well-behaved-but-unreachable case");
        Assert.That(result.NonFiniteGenerations, Is.EqualTo(3), "every generation is scored against the same NaN target, so every one of the 3 executed generations must be affected");
    }
}

/// <summary>
/// pins <see cref="BenchmarkProblem.CountNonFiniteGenerations"/>
/// </summary>
[TestFixture, Parallelizable]
public class BenchmarkProblemCountNonFiniteGenerationsTests {

    [Test, Parallelizable]
    [Description("An empty observation sequence counts zero affected generations. DiVoid #9511.")]
    public void CountNonFiniteGenerations_EmptySequence_ReturnsZero() {
        Assert.That(BenchmarkProblem.CountNonFiniteGenerations([]), Is.Zero);
    }

    [Test, Parallelizable]
    [Description("All-zero per-generation counts count zero affected generations, killing a '>= 0' mutation of the '> 0' threshold. DiVoid #9511.")]
    public void CountNonFiniteGenerations_AllZeroCounts_ReturnsZero() {
        Assert.That(BenchmarkProblem.CountNonFiniteGenerations([0, 0, 0, 0]), Is.Zero);
    }

    [Test, Parallelizable]
    [Description("Every generation reporting a nonzero count counts as affected - the fully-persistent case. DiVoid #9511.")]
    public void CountNonFiniteGenerations_EveryGenerationNonZero_ReturnsGenerationCount() {
        Assert.That(BenchmarkProblem.CountNonFiniteGenerations([1, 2, 3]), Is.EqualTo(3));
    }

    [Test, Parallelizable]
    [Description("Counts affected generations, not affected entries: [0,3,0,1,0] is 2, not 4 (sum) or 1 (any-check). DiVoid #9511.")]
    public void CountNonFiniteGenerations_MixedSequence_CountsAffectedGenerationsNotAffectedEntries() {
        Assert.That(BenchmarkProblem.CountNonFiniteGenerations([0, 3, 0, 1, 0]), Is.EqualTo(2));
    }
}
