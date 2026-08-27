using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class BreedingWeightTests {

    [Test, Parallelizable]
    [TestCase(0.5f, "e0'1")]
    [TestCase(0.97f, "e1'1")]
    [Description("Independent R6 oracle: hand-computed breeding weights and cumulative selectors over three different FitnessModifier values predict which parent the gene-pool slot draws, and the two probes draw different parents.")]
    public void Evolve_ScriptedRngDrivingGenePool_DrawsTheParentTheCumulativeSelectorArithmeticPredicts(float selector, string expectedChild) {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = new() { Chromosome = new("e0", log, 0, 2.0f), Fitness = 1.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e1 = new() { Chromosome = new("e1", log, 1, 1.0f), Fitness = 3.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e2 = new() { Chromosome = new("e2", log, 2, 0.2f), Fitness = 0.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 1.0f,
            ["e1"] = 3.0f,
            ["e2"] = 0.0f,
            ["e0'1"] = 10.0f,
            ["e1'1"] = 10.0f,
            ["fresh'1"] = 10.0f,
            ["e0'2"] = 11.0f,
            ["e1'2"] = 11.0f,
            ["fresh'2"] = 11.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [selector, selector] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label),
                    Is.EquivalentTo(new[] { "e0", expectedChild, "fresh'2" }),
                    "the gene-pool slot must draw the parent the modifier-and-squaring-aware transform predicts");
    }
}
