using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class EvolveMechanicsTests {

    static PopulationEntry<MutatingFakeChromosome> Entry(string label, List<MutateCall> log, float fitness, int structureHash) => new() {
        Chromosome = new(label, log, structureHash, 1.0f),
        Fitness = fitness,
        AncestryId = Guid.NewGuid()
    };


    [Test, Parallelizable]
    public void Evolve_ElitismKWithKPlusNDistinctNonNegativeEntries_ExactlyBestKSurviveByReferenceIdentity() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome> e2 = Entry("e2", log, 5.0f, 2);
        PopulationEntry<MutatingFakeChromosome> e3 = Entry("e3", log, 6.0f, 3);
        PopulationEntry<MutatingFakeChromosome> e4 = Entry("e4", log, 7.0f, 4);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2, e3, e4];

        int freshCounter = 0;
        Population<MutatingFakeChromosome> population = new(entries, r => new($"fresh{freshCounter++}", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 0.0f,
            ["e1"] = 1.0f,
            ["e2"] = 5.0f,
            ["e3"] = 6.0f,
            ["e4"] = 7.0f,
            ["e0'1"] = 10.0f,
            ["e0'2"] = 11.0f,
            ["fresh0'3"] = 12.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0, 0) { FloatValues = [0.0f, 0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 2,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries, Has.Member(e0));
        Assert.That(population.Entries, Has.Member(e1));
        Assert.That(population.Entries[0], Is.SameAs(e0));
        Assert.That(population.Entries[1], Is.SameAs(e1));
    }


    [Test, Parallelizable]
    public void Evolve_DuplicateStructureHashAmongLeaders_ElitistBandIsCorrespondinglyShorter() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 7);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 7);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1];

        int freshCounter = 0;
        Population<MutatingFakeChromosome> population = new(entries, r => new($"fresh{freshCounter++}", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 0.0f,
            ["e1"] = 1.0f,
            ["fresh0'1"] = 5.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0) { FloatValues = [] },
            Threads = 1,
            Runs = 1,
            Elitism = 2,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries, Has.Length.EqualTo(2), "population size must be preserved");
        Assert.That(population.Entries, Has.Member(e0), "the first-seen weight-variant of the duplicated topology must survive as elite");
        Assert.That(population.Entries, Has.No.Member(e1), "the second weight-variant of the SAME topology must not occupy a second elite slot");
    }


    [Test, Parallelizable]
    public void Evolve_NegativeFitnessEntry_ExcludedFromElitismAndGenePool() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> negative = Entry("negative", log, -1.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome> e2 = Entry("e2", log, 2.0f, 2);
        PopulationEntry<MutatingFakeChromosome>[] entries = [negative, e1, e2];

        int freshCounter = 0;
        Population<MutatingFakeChromosome> population = new(entries, r => new($"fresh{freshCounter++}", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["negative"] = -1.0f,
            ["e1"] = 1.0f,
            ["e2"] = 2.0f,
            ["fresh0'1"] = 3.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0) { FloatValues = [] },
            Threads = 1,
            Runs = 1,
            Elitism = 2,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries, Has.No.Member(negative), "a negative-fitness entry must not be carried forward as elite");
        Assert.That(population.Entries, Has.Member(e1));
        Assert.That(population.Entries, Has.Member(e2));
    }


    [Test, Parallelizable]
    [Description("Re-scored fitness values are deliberately different from, and not in ascending order relative to, the construction values, so the final ordering can only be explained by the post-Evolve score - negative special-cased to sort last - not by fixture order or magnitude (R2).")]
    public void Evolve_AfterOneGeneration_EntriesAreAscendingByFitnessWithNegativesLast() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome> e2 = Entry("e2", log, 2.0f, 2);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        int freshCounter = 0;
        Population<MutatingFakeChromosome> population = new(entries, r => new($"fresh{freshCounter++}", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 5.0f,
            ["e1"] = 1.0f,
            ["e2"] = 2.0f,
            ["e0'1"] = -3.0f,
            ["e0'2"] = 2.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.0f, 0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries, Has.Length.EqualTo(3));
        Assert.That(population.Entries[0].Chromosome.Label, Is.EqualTo("e0'2"), "2.0 is the lowest non-negative fitness");
        Assert.That(population.Entries[1].Chromosome.Label, Is.EqualTo("e0"), "5.0 is the second-lowest non-negative fitness");
        Assert.That(population.Entries[2].Chromosome.Label, Is.EqualTo("e0'1"), "a negative fitness must sort last regardless of magnitude");
        Assert.That(population.Entries[2].Fitness, Is.EqualTo(-3.0f), "the raw fitness value is preserved even though it sorts last");
    }
}
