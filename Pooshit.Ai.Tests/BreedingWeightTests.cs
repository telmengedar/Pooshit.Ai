using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class BreedingWeightTests {

    [Test, Parallelizable]
    [Description("Independent R6 oracle: hand-computed breeding weights and cumulative selectors (design #9072 §10.1) predict which parent GenePool.Next draws for each mutate slot, rather than reading the value back out of the system under test.")]
    public void Evolve_ScriptedRngDrivingGenePool_DrawsTheParentTheCumulativeSelectorArithmeticPredicts() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = new() { Chromosome = new("e0", log, 0, 1.0f), Fitness = 0.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e1 = new() { Chromosome = new("e1", log, 1, 1.0f), Fitness = 1.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e2 = new() { Chromosome = new("e2", log, 2, 1.0f), Fitness = 3.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 0.0f,
            ["e1"] = 1.0f,
            ["e2"] = 3.0f,
            ["e1'1"] = 10.0f,
            ["e0'2"] = 11.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.9f, 0.5f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label),
                    Is.EquivalentTo(new[] { "e0", "e1'1", "e0'2" }),
                    "slot 1 must be a child of e1 (predicted parent) and slot 2 a child of e0 (predicted parent)");
    }
}
