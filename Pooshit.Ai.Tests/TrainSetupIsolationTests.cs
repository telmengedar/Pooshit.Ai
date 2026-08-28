using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// pins that <see cref="Population{T}.Train"/> treats its <see cref="EvolutionSetup{T}"/> as input
/// only, under both reproduction strategies and across repeated calls
/// </summary>
[TestFixture, Parallelizable]
public class TrainSetupIsolationTests {
    const long Seed = 20260828;

    static PopulationEntry<T> Entry<T>(T chromosome)
    where T : IChromosome<T> => new() {
        Chromosome = chromosome,
        AncestryId = Guid.NewGuid()
    };

    static EvolutionSetup<T> Setup<T>(int mutationRuns, int generations, int elitism)
    where T : IChromosome<T> {
        EvolutionSetup<T> setup = new() {
            Evaluator = new PhaseAwareFitnessEvaluator<T>((_, _) => 1.0f),
            Threads = 1,
            Runs = generations,
            Elitism = elitism,
            TargetFitness = -1.0f
        };
        setup.Mutation.Runs = mutationRuns;
        return setup;
    }

    static List<int> TrainFreshMutatePopulation(EvolutionSetup<MutatingFakeChromosome> setup) {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome>[] entries = [
            Entry(new MutatingFakeChromosome("e0", log)),
            Entry(new MutatingFakeChromosome("e1", log))
        ];
        Population<MutatingFakeChromosome> population = new(entries, rng => new("fresh", log));
        RecordingRng recordingRng = new(new Rng(Seed));
        setup.Rng = recordingRng;

        population.Train(setup);
        return recordingRng.Bounds;
    }


    [Test, Parallelizable]
    [TestCase(3)]
    [TestCase(7)]
    [Description("Train keeps the escalated stagnation run count in local training state, so the caller's configured Mutation.Runs survives a mutate-strategy run. DiVoid #9054 item 1.")]
    public void Train_MutateStrategy_LeavesConfiguredMutationRunsUnchanged(int mutationRuns) {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome>[] entries = [
            Entry(new MutatingFakeChromosome("e0", log)),
            Entry(new MutatingFakeChromosome("e1", log))
        ];
        Population<MutatingFakeChromosome> population = new(entries, rng => new("fresh", log));
        EvolutionSetup<MutatingFakeChromosome> setup = Setup<MutatingFakeChromosome>(mutationRuns, 2, 1);
        setup.Rng = new Rng(Seed);

        population.Train(setup);

        Assert.That(setup.Mutation.Runs, Is.EqualTo(mutationRuns));
    }


    [Test, Parallelizable]
    [TestCase(3)]
    [TestCase(7)]
    [Description("The cross strategy never reads Mutation.Runs, so a cross-strategy run leaves the caller's configured value untouched rather than overwriting a knob it does not consume. DiVoid #9054 item 1.")]
    public void Train_CrossStrategy_LeavesConfiguredMutationRunsUnchanged(int mutationRuns) {
        List<CrossCall> log = [];
        PopulationEntry<CrossingFakeChromosome>[] entries = [
            Entry(new CrossingFakeChromosome("e0", log)),
            Entry(new CrossingFakeChromosome("e1", log))
        ];
        Population<CrossingFakeChromosome> population = new(entries, rng => new("fresh", log));
        EvolutionSetup<CrossingFakeChromosome> setup = Setup<CrossingFakeChromosome>(mutationRuns, 2, 1);
        setup.Rng = new Rng(Seed);

        population.Train(setup);

        Assert.That(setup.Mutation.Runs, Is.EqualTo(mutationRuns));
    }


    [Test, Parallelizable]
    [TestCase(3)]
    [TestCase(7)]
    [Description("Two identical runs sharing one EvolutionSetup request the same mutation depth bound, so a second Train starts from the configured value rather than from the depth the first run escalated to. DiVoid #9054 item 1.")]
    public void Train_TwiceWithTheSameSetup_BothRunsRequestTheConfiguredMutationDepthBound(int mutationRuns) {
        EvolutionSetup<MutatingFakeChromosome> setup = Setup<MutatingFakeChromosome>(mutationRuns, 1, 0);

        List<int> firstRunBounds = TrainFreshMutatePopulation(setup);
        List<int> secondRunBounds = TrainFreshMutatePopulation(setup);

        Assert.That(firstRunBounds, Is.Not.Empty, "the mutate strategy must have drawn a mutation depth at least once");
        Assert.That(firstRunBounds, Is.All.EqualTo(mutationRuns));
        Assert.That(secondRunBounds, Is.EqualTo(firstRunBounds));
    }
}
