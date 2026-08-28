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
    [Ignore("DiVoid #9047 - rivalism mutates cumulatively across rival iterations (each rival mutates the previous rival's result, not fresh from the parent), so independent sampling is not yet the case")]
    [Description("Intended contract: with Rivalism = r, each of the r rivals is an independent mutation of the SAME parent (not a chain), and the best-scoring rival is kept. DiVoid #9047.")]
    public void Evolve_RivalismGreaterThanOne_IntendedToSampleRivalsIndependentlyAndKeepTheBest() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome> e2 = Entry("e2", log, 2.0f, 2);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 0.0f,
            ["e1"] = 1.0f,
            ["e2"] = 2.0f,
            ["e0'1"] = 20.0f,
            ["e0'2"] = 5.0f,
            ["e0'3"] = 20.0f,
            ["e0'4"] = 5.0f,
            ["fresh'1"] = 30.0f,
            ["fresh'2"] = 31.0f,
            ["fresh'3"] = 30.0f,
            ["fresh'4"] = 31.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0, 0, 0) { FloatValues = [0.0f, 0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            Rivalism = 2,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        int distinctCandidatesEvaluated = evaluator.Calls
                                                    .Select(call => call.Label)
                                                    .Where(label => label.StartsWith("e0'"))
                                                    .Distinct()
                                                    .Count();
        Assert.That(distinctCandidatesEvaluated, Is.EqualTo(setup.Rivalism));
        Assert.That(population.Entries.Any(entry => entry.Chromosome.Label == "e0'2"), Is.True, "the better-scoring rival must be kept");
        Assert.That(population.Entries.Any(entry => entry.Chromosome.Label == "e0'1"), Is.False, "the worse-scoring rival must be discarded");
    }


    [Test, Parallelizable]
    [Description("Documents CURRENT (defective) behaviour, not the contract: rivalism mutates cumulatively, so the lineage is e0 -> e0'1 (rival 1, worse) -> e0'1'2 (rival 2, better) rather than two independent children of e0. DiVoid #9047. This test is expected to go red the day #9047 is fixed - see the sibling intended-contract pin above.")]
    public void Evolve_RivalismGreaterThanOne_CandidatesCurrentlyChainCumulatively() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome> e2 = Entry("e2", log, 2.0f, 2);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 0.0f,
            ["e1"] = 1.0f,
            ["e2"] = 2.0f,
            ["e0'1"] = 20.0f,
            ["e0'1'2"] = 5.0f,
            ["e0'2"] = 5.0f,
            ["e0'3"] = 20.0f,
            ["e0'3'4"] = 5.0f,
            ["fresh'1"] = 30.0f,
            ["fresh'1'2"] = 31.0f,
            ["fresh'2"] = 31.0f,
            ["fresh'3"] = 30.0f,
            ["fresh'3'4"] = 31.0f,
            ["fresh'4"] = 31.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0, 0, 0) { FloatValues = [0.0f, 0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            Rivalism = 2,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        int distinctCandidatesEvaluated = evaluator.Calls
                                                    .Select(call => call.Label)
                                                    .Where(label => label.StartsWith("e0'"))
                                                    .Distinct()
                                                    .Count();
        Assert.That(distinctCandidatesEvaluated, Is.EqualTo(setup.Rivalism));
        Assert.That(population.Entries.Any(entry => entry.Chromosome.Label == "e0'1'2"), Is.True, "the better-scoring rival must be kept");
        Assert.That(population.Entries.Any(entry => entry.Chromosome.Label == "e0'1"), Is.False, "the worse-scoring rival must be discarded");
    }
}
