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
    [Description("Sibling to the case above, same fixture shape: a well-behaved reachable-by-construction sample never produces a non-finite fitness, so NonFiniteGenerations must be zero (DiVoid #9511). Pairs with the NaN-target case below to prove the field actually varies with what the run observes (R1), rather than being pinned regardless of input.")]
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
    [Description("SamplesEvaluator divides by MathF.Max(Abs(expected), 1.0f), never by the expected value itself, so an expected output of NaN is the cleanest known trigger for a non-finite fitness: 'anything minus NaN' is NaN regardless of chromosome structure, on every entry, every generation, independent of seed. Proves BenchmarkProblem.Run() actually wires OnNonFiniteFitness through to NonFiniteGenerations end-to-end (DiVoid #9511) rather than leaving the field always zero - a defect the two Median-style pure-function tests on CountNonFiniteGenerations cannot catch, since they never touch the wiring.")]
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
/// pins <see cref="BenchmarkProblem.CountNonFiniteGenerations"/> - the persistence rule DiVoid
/// #9511 asks for (count affected GENERATIONS, never affected ENTRIES), extracted as a pure
/// function exactly as <see cref="BenchmarkReport.Median"/> was, because <see cref="BenchmarkProblemTests"/>
/// above cannot deterministically drive a real training run through a chosen mix of affected and
/// unaffected generations - the accumulation rule itself is tested directly here instead
/// </summary>
[TestFixture, Parallelizable]
public class BenchmarkProblemCountNonFiniteGenerationsTests {

    [Test, Parallelizable]
    [Description("No generations observed at all (an empty run) counts zero affected generations, not a default or an exception.")]
    public void CountNonFiniteGenerations_EmptySequence_ReturnsZero() {
        Assert.That(BenchmarkProblem.CountNonFiniteGenerations([]), Is.Zero);
    }

    [Test, Parallelizable]
    [Description("A sequence of all-zero per-generation counts (nothing ever observed) counts zero affected generations - kills a '>= 0' mutation of the '> 0' threshold, which would otherwise count every generation regardless of whether anything was actually observed.")]
    public void CountNonFiniteGenerations_AllZeroCounts_ReturnsZero() {
        Assert.That(BenchmarkProblem.CountNonFiniteGenerations([0, 0, 0, 0]), Is.Zero);
    }

    [Test, Parallelizable]
    [Description("Every observed generation reporting a nonzero count must itself count as affected - the fully-persistent case, and the sibling that proves the rule does not always return a constant regardless of input (R1).")]
    public void CountNonFiniteGenerations_EveryGenerationNonZero_ReturnsGenerationCount() {
        Assert.That(BenchmarkProblem.CountNonFiniteGenerations([1, 2, 3]), Is.EqualTo(3));
    }

    [Test, Parallelizable]
    [Description("The rule counts affected GENERATIONS, not affected ENTRIES: a generation reporting 3 non-finite entries and one reporting 1 must each count as exactly one generation, so [0,3,0,1,0] is 2 - never 4 (the sum of the counts) and never 1 (a saturating any-nonzero-ever check collapsing to a boolean). This is the exact distinction DiVoid #9511 draws between a persistence signal and a raw total, and it is the test that kills a 'Sum instead of Count' mutation.")]
    public void CountNonFiniteGenerations_MixedSequence_CountsAffectedGenerationsNotAffectedEntries() {
        Assert.That(BenchmarkProblem.CountNonFiniteGenerations([0, 3, 0, 1, 0]), Is.EqualTo(2));
    }
}
