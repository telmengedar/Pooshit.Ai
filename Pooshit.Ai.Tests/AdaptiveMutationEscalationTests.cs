using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class AdaptiveMutationEscalationTests {

    static PopulationEntry<MutatingFakeChromosome> Entry(string label, List<MutateCall> log, float fitness, int structureHash) => new() {
        Chromosome = new(label, log, structureHash, 1.0f),
        Fitness = fitness,
        AncestryId = Guid.NewGuid()
    };


    [Test, Parallelizable]
    [Description("Every chromosome shares one StructureHash across 70 generations so only Train's escalation branch (never the reset branch) can fire, pinning the documented schedule Math.Min(50, 1 + ((i - bestRun) >> 6) * 5).")]
    public void Train_LeaderStructureHashUnchangedFor64Generations_MutationRunsEscalatesOnDocumentedSchedule() {
        List<MutateCall> log = [];
        const int constantStructureHash = 42;
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, constantStructureHash);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, constantStructureHash);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, constantStructureHash, 1.0f));

        List<int> mutationRunsAfterEachGeneration = [];
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = new ConstantFitnessEvaluator<MutatingFakeChromosome>(1.0f),
            Rng = new Rng(20260824),
            Threads = 1,
            Runs = 70,
            Elitism = 1,
            TargetFitness = -1.0f
        };
        setup.AfterRun = (generation, fitness) => mutationRunsAfterEachGeneration.Add(setup.Mutation.Runs);

        population.Train(setup);

        Assert.That(mutationRunsAfterEachGeneration, Has.Count.EqualTo(70));
        Assert.That(mutationRunsAfterEachGeneration[63], Is.EqualTo(1), "generation 63 (i - bestRun = 63) is still below the 64-generation escalation threshold");
        Assert.That(mutationRunsAfterEachGeneration[64], Is.EqualTo(6), "generation 64 (i - bestRun = 64) crosses the threshold: 1 + (64 >> 6) * 5 = 6");
    }


    [Test, Parallelizable]
    [Description("The fresh-blood slot's generator (StructureHash 99) is scored best by the evaluator and becomes the new leader, forcing a structure change so Mutation.Runs must reset from a deliberately pre-set 7 to 1.")]
    public void Train_LeaderStructureHashChanges_MutationRunsResetsToOne() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 1);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 2);
        PopulationEntry<MutatingFakeChromosome> e2 = Entry("e2", log, 2.0f, 3);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        Population<MutatingFakeChromosome> population = new(entries, r => new("FRESH", log, 99, 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 5.0f,
            ["e1"] = 6.0f,
            ["e2"] = 2.0f,
            ["FRESH'1"] = 0.0f
        };

        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = new StubFitnessEvaluator<MutatingFakeChromosome>(fitnessByLabel),
            Rng = new SequenceRng(0),
            Threads = 1,
            Runs = 1,
            Elitism = 2,
            TargetFitness = -1.0f
        };
        setup.Mutation.Runs = 7;

        population.Train(setup);

        Assert.That(population.Entries[0].Chromosome.Label, Does.StartWith("FRESH"), "the fresh-blood-derived entry must have won this generation");
        Assert.That(setup.Mutation.Runs, Is.EqualTo(1));
    }
}
