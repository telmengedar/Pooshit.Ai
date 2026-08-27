using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class FreshBloodBandTests {

    static PopulationEntry<MutatingFakeChromosome> Entry(string label, List<MutateCall> log, float fitness, int structureHash) => new() {
        Chromosome = new(label, log, structureHash, 1.0f),
        Fitness = fitness,
        AncestryId = Guid.NewGuid()
    };


    [Test, Parallelizable]
    [Description("Pins the fresh-blood band width: the slots drawn from Generator rather than the gene pool number exactly Elitism (DiVoid #9054).")]
    public void Evolve_FreshBloodBand_MarksExactlyElitismSlots() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome> e2 = Entry("e2", log, 2.0f, 2);
        PopulationEntry<MutatingFakeChromosome> e3 = Entry("e3", log, 3.0f, 3);
        PopulationEntry<MutatingFakeChromosome> e4 = Entry("e4", log, 4.0f, 4);
        PopulationEntry<MutatingFakeChromosome> e5 = Entry("e5", log, 5.0f, 5);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2, e3, e4, e5];

        int freshCounter = 0;
        Population<MutatingFakeChromosome> population = new(entries, r => new($"FRESH{freshCounter++}", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 0.0f,
            ["e1"] = 1.0f,
            ["e2"] = 2.0f,
            ["e3"] = 3.0f,
            ["e4"] = 4.0f,
            ["e5"] = 5.0f,
            ["e0'1"] = 10.0f,
            ["FRESH0'2"] = 11.0f,
            ["FRESH1'3"] = 12.0f,
            ["FRESH0'1"] = 13.0f,
            ["FRESH1'2"] = 14.0f,
            ["FRESH2'3"] = 15.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0, 0) { FloatValues = [0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 3,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        int freshBloodSlots = population.Entries.Count(entry => entry.Chromosome.Label.Contains("FRESH"));
        Assert.That(freshBloodSlots, Is.EqualTo(setup.Elitism));
    }
}
