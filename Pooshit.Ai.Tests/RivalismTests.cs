using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class RivalismTests {

    static PopulationEntry<MutatingFakeChromosome> Entry(string label, List<MutateCall> log, float fitness, int structureHash) => new() {
        Chromosome = new(label, log, structureHash, 1.0f),
        Fitness = fitness,
        AncestryId = Guid.NewGuid()
    };


    [Test, Parallelizable]
    public void Evolve_RivalismGreaterThanOne_EvaluatesExactlyRivalismCandidatesAndKeepsTheBest() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        // candidate lineage: parent "e0" -> "e0'1" (rival 1, worse) -> "e0'1'2" (rival 2, kept - the best)
        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 0.0f,
            ["e1"] = 1.0f,
            ["e0'1"] = 20.0f,   // rival 1 - worse
            ["e0'1'2"] = 5.0f   // rival 2 - better, must be kept
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            Rivalism = 2,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        int distinctCandidatesEvaluated = evaluator.Calls
                                                    .Select(call => call.Label)
                                                    .Where(label => label is "e0'1" or "e0'1'2")
                                                    .Distinct()
                                                    .Count();
        Assert.That(distinctCandidatesEvaluated, Is.EqualTo(setup.Rivalism));
        Assert.That(population.Entries.Any(entry => entry.Chromosome.Label == "e0'1'2"), Is.True, "the better-scoring rival must be kept");
        Assert.That(population.Entries.Any(entry => entry.Chromosome.Label == "e0'1"), Is.False, "the worse-scoring rival must be discarded");
    }
}
