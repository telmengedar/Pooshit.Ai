using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// pins which <see cref="EvolutionSetup{T}"/> knobs each reproduction strategy reads, and which it
/// leaves without observable effect
/// </summary>
[TestFixture, Parallelizable]
public class StrategySpecificSetupTests {
    const float LowRate = 0.05f;
    const float HighRate = 0.5f;
    const float LowRange = 0.2f;
    const float HighRange = 0.8f;
    const float LowChance = 0.1f;
    const float HighChance = 0.9f;
    const float LowIncestFactor = 2.0f;
    const float HighIncestFactor = 20.0f;

    static float[] DistinctParents => [0.0f, 0.9f, 0.0f, 0.9f, 0.0f, 0.9f];

    static float[] TiedParents => [0.0f, 0.0f, 0.9f, 0.9f, 0.0f, 0.0f];

    static float[] ParentPerSlot => [0.0f, 0.0f, 0.0f];

    static Dictionary<string, float> CrossFitness() => new() {
        ["e0"] = 0.0f,
        ["e1"] = 1.0f,
        ["e2"] = 3.0f,
        ["e0xe0"] = 0.5f,
        ["e0xe1"] = 0.5f,
        ["e1xe1"] = 0.5f
    };

    static Dictionary<string, float> MutateFitness() => new() {
        ["e0"] = 0.0f,
        ["e1"] = 1.0f,
        ["e2"] = 3.0f,
        ["e0'1"] = 1.0f,
        ["e0'2"] = 2.0f,
        ["e0'3"] = 3.0f,
        ["e0'4"] = 4.0f,
        ["e0'5"] = 5.0f,
        ["e0'6"] = 6.0f,
        ["e0'7"] = 7.0f,
        ["e0'8"] = 8.0f,
        ["e0'9"] = 9.0f
    };

    static PopulationEntry<T> Entry<T>(T chromosome)
    where T : IChromosome<T> => new() {
        Chromosome = chromosome,
        AncestryId = Guid.NewGuid()
    };

    static List<CrossCall> RunCross(float[] poolDraws, Action<EvolutionSetup<CrossingFakeChromosome>> configure) {
        List<CrossCall> log = [];
        PopulationEntry<CrossingFakeChromosome>[] entries = [
            Entry(new CrossingFakeChromosome("e0", log, 0)),
            Entry(new CrossingFakeChromosome("e1", log, 1)),
            Entry(new CrossingFakeChromosome("e2", log, 2))
        ];
        Population<CrossingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));
        EvolutionSetup<CrossingFakeChromosome> setup = new() {
            Evaluator = new StubFitnessEvaluator<CrossingFakeChromosome>(CrossFitness()),
            Rng = new SequenceRng { FloatValues = poolDraws },
            Threads = 1,
            Runs = 1,
            Elitism = 0,
            TargetFitness = -1.0f
        };
        configure(setup);

        population.Train(setup);
        return log;
    }

    static (List<MutateCall> Log, List<int> RequestedRunBounds) RunMutate(int[] runDraws, Action<EvolutionSetup<MutatingFakeChromosome>> configure) {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome>[] entries = [
            Entry(new MutatingFakeChromosome("e0", log, 0)),
            Entry(new MutatingFakeChromosome("e1", log, 1)),
            Entry(new MutatingFakeChromosome("e2", log, 2))
        ];
        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));
        SequenceRng rng = new(runDraws) { FloatValues = ParentPerSlot };
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = new StubFitnessEvaluator<MutatingFakeChromosome>(MutateFitness()),
            Rng = rng,
            Threads = 1,
            Runs = 1,
            Elitism = 0,
            TargetFitness = -1.0f
        };
        configure(setup);

        population.Train(setup);
        return (log, rng.Bounds);
    }

    static void AssertScalesWithSlotIndex(float[] perSlot) {
        for (int slot = 1; slot < perSlot.Length; ++slot)
            Assert.That(perSlot[slot], Is.GreaterThan(perSlot[slot - 1]), $"slot {slot} must carry a larger scaled value than slot {slot - 1}");

        Assert.That(perSlot[^1], Is.GreaterThan(0.0f), "the last reproduced slot never sits at index zero, so its scaled value must be positive");
    }

    static void AssertCrossPathDidRealWork(List<CrossCall> log) {
        Assert.That(log, Is.Not.Empty, "the cross strategy must have reproduced at least one slot");
        Assert.That(log.Select(call => (call.FirstLabel, call.SecondLabel)), Is.All.EqualTo(("e0", "e1")));
        AssertScalesWithSlotIndex(log.Select(call => call.MutateRange).ToArray());
    }

    static void AssertMutatePathDidRealWork(List<MutateCall> log) {
        Assert.That(log, Is.Not.Empty, "the mutate strategy must have reproduced at least one slot");
        Assert.That(log.Select(call => call.ReceiverLabel), Is.All.EqualTo("e0"));
        AssertScalesWithSlotIndex(log.Select(call => call.Range).ToArray());
    }


    [Test, Parallelizable]
    [Description("Rivalism has no observable effect under the cross strategy: two runs differing only in it reproduce identically. DiVoid #9925.")]
    public void Cross_RivalismVaried_ReproducesIdentically() {
        List<CrossCall> baseline = RunCross(DistinctParents, setup => setup.Rivalism = 1);
        List<CrossCall> varied = RunCross(DistinctParents, setup => setup.Rivalism = 7);

        AssertCrossPathDidRealWork(baseline);
        Assert.That(varied, Is.EqualTo(baseline));
    }


    [Test, Parallelizable]
    [Description("Sibling to the cross-inertness pin: under the mutate strategy Rivalism does move the observable, producing one rival chain per rival per reproduced slot. DiVoid #9925.")]
    public void Mutate_RivalismVaried_ChangesNumberOfRivalChains() {
        (List<MutateCall> singleRival, _) = RunMutate([0, 0, 0], setup => setup.Rivalism = 1);
        (List<MutateCall> threeRivals, _) = RunMutate([0, 0, 0, 0, 0, 0, 0, 0, 0], setup => setup.Rivalism = 3);

        Assert.That(singleRival, Is.Not.Empty, "the mutate strategy must have reproduced at least one slot");
        Assert.That(threeRivals, Has.Count.EqualTo(3 * singleRival.Count));
    }


    [Test, Parallelizable]
    [Description("Mutation.Runs has no observable effect under the cross strategy: two runs differing only in it reproduce identically. DiVoid #9925.")]
    public void Cross_MutationRunsVaried_ReproducesIdentically() {
        List<CrossCall> baseline = RunCross(DistinctParents, setup => setup.Mutation.Runs = 1);
        List<CrossCall> varied = RunCross(DistinctParents, setup => setup.Mutation.Runs = 9);

        AssertCrossPathDidRealWork(baseline);
        Assert.That(varied, Is.EqualTo(baseline));
    }


    [Test, Parallelizable]
    [Description("Sibling to the cross-inertness pin: under the mutate strategy Mutation.Runs is the upper bound of the drawn per-chain mutation count. DiVoid #9925.")]
    public void Mutate_MutationRunsVaried_ChangesRequestedRunBound() {
        (_, List<int> lowBounds) = RunMutate([0, 0, 0], setup => setup.Mutation.Runs = 1);
        (_, List<int> highBounds) = RunMutate([0, 0, 0], setup => setup.Mutation.Runs = 9);

        Assert.That(lowBounds, Is.Not.Empty, "the mutate strategy must have drawn a run count at least once");
        Assert.That(lowBounds, Is.All.EqualTo(1));
        Assert.That(highBounds, Has.Count.EqualTo(lowBounds.Count));
        Assert.That(highBounds, Is.All.EqualTo(9));
    }


    [Test, Parallelizable]
    [Description("Mutation.Chance has no observable effect under the mutate strategy: two runs differing only in it reproduce identically. DiVoid #9925.")]
    public void Mutate_MutationChanceVaried_ReproducesIdentically() {
        (List<MutateCall> baseline, _) = RunMutate([0, 0, 0], setup => setup.Mutation.Chance = LowChance);
        (List<MutateCall> varied, _) = RunMutate([0, 0, 0], setup => setup.Mutation.Chance = HighChance);

        AssertMutatePathDidRealWork(baseline);
        Assert.That(varied, Is.EqualTo(baseline));
    }


    [Test, Parallelizable]
    [Description("Sibling to the mutate-inertness pin: under the cross strategy Mutation.Chance reaches the chromosome verbatim as CrossSetup.MutateChance. DiVoid #9925.")]
    public void Cross_MutationChanceVaried_ChangesCrossMutateChance() {
        List<CrossCall> low = RunCross(DistinctParents, setup => setup.Mutation.Chance = LowChance);
        List<CrossCall> high = RunCross(DistinctParents, setup => setup.Mutation.Chance = HighChance);

        Assert.That(low, Is.Not.Empty);
        Assert.That(low.Select(call => call.MutateChance), Is.All.EqualTo(LowChance));
        Assert.That(high.Select(call => call.MutateChance), Is.All.EqualTo(HighChance));
    }


    [Test, Parallelizable]
    [Description("Mutation.Rate has no observable effect under the mutate strategy: two runs differing only in it reproduce identically. DiVoid #9925.")]
    public void Mutate_MutationRateVaried_ReproducesIdentically() {
        (List<MutateCall> baseline, _) = RunMutate([0, 0, 0], setup => setup.Mutation.Rate = LowRate);
        (List<MutateCall> varied, _) = RunMutate([0, 0, 0], setup => setup.Mutation.Rate = HighRate);

        AssertMutatePathDidRealWork(baseline);
        Assert.That(varied, Is.EqualTo(baseline));
    }


    [Test, Parallelizable]
    [Description("Sibling to the mutate-inertness pin: under the cross strategy Mutation.Rate scales the CrossSetup.MutateRate the chromosome receives. DiVoid #9925.")]
    public void Cross_MutationRateVaried_ChangesCrossMutateRate() {
        List<CrossCall> low = RunCross(DistinctParents, setup => setup.Mutation.Rate = LowRate);
        List<CrossCall> high = RunCross(DistinctParents, setup => setup.Mutation.Rate = HighRate);

        Assert.That(low[^1].MutateRate, Is.GreaterThan(0.0f));
        Assert.That(high[^1].MutateRate, Is.GreaterThan(low[^1].MutateRate));
    }


    [Test, Parallelizable]
    [Description("Mutation.IncestFactor has no observable effect under the mutate strategy: two runs differing only in it reproduce identically. DiVoid #9925.")]
    public void Mutate_IncestFactorVaried_ReproducesIdentically() {
        (List<MutateCall> baseline, _) = RunMutate([0, 0, 0], setup => setup.Mutation.IncestFactor = LowIncestFactor);
        (List<MutateCall> varied, _) = RunMutate([0, 0, 0], setup => setup.Mutation.IncestFactor = HighIncestFactor);

        AssertMutatePathDidRealWork(baseline);
        Assert.That(varied, Is.EqualTo(baseline));
    }


    [Test, Parallelizable]
    [Description("Sibling to the mutate-inertness pin: under the cross strategy Mutation.IncestFactor scales mutation up when two equally fit parents are drawn. DiVoid #9925.")]
    public void Cross_IncestFactorVaried_ChangesCrossMutateRateForTiedParents() {
        List<CrossCall> low = RunCross(TiedParents, setup => {
            setup.Mutation.Rate = LowRate;
            setup.Mutation.IncestFactor = LowIncestFactor;
        });
        List<CrossCall> high = RunCross(TiedParents, setup => {
            setup.Mutation.Rate = LowRate;
            setup.Mutation.IncestFactor = HighIncestFactor;
        });

        Assert.That(low, Is.Not.Empty);
        Assert.That(low.Select(call => call.FirstLabel == call.SecondLabel), Is.All.True);
        Assert.That(low.Select(call => call.MutateChance), Is.All.EqualTo(1.0f));
        Assert.That(low[^1].MutateRate, Is.GreaterThan(0.0f));
        Assert.That(high[^1].MutateRate, Is.GreaterThan(low[^1].MutateRate));
    }


    [Test, Parallelizable]
    [Description("Mutation.Range is read by the cross strategy, scaling the CrossSetup.MutateRange the chromosome receives. DiVoid #9925.")]
    public void Cross_MutationRangeVaried_ChangesCrossMutateRange() {
        List<CrossCall> low = RunCross(DistinctParents, setup => setup.Mutation.Range = LowRange);
        List<CrossCall> high = RunCross(DistinctParents, setup => setup.Mutation.Range = HighRange);

        Assert.That(low[^1].MutateRange, Is.GreaterThan(0.0f));
        Assert.That(high[^1].MutateRange, Is.GreaterThan(low[^1].MutateRange));
    }


    [Test, Parallelizable]
    [Description("Mutation.Range is read by the mutate strategy too, scaling the range each mutation call receives. DiVoid #9925.")]
    public void Mutate_MutationRangeVaried_ChangesMutationRange() {
        (List<MutateCall> low, _) = RunMutate([0, 0, 0], setup => setup.Mutation.Range = LowRange);
        (List<MutateCall> high, _) = RunMutate([0, 0, 0], setup => setup.Mutation.Range = HighRange);

        Assert.That(low[^1].Range, Is.GreaterThan(0.0f));
        Assert.That(high[^1].Range, Is.GreaterThan(low[^1].Range));
    }
}
