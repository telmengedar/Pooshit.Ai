using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class BreedingWeightTests {

    [Test, Parallelizable]
    [Description("Independent R6 oracle: hand-computed breeding weights and cumulative selectors (design #9072 §10.1), using THREE DIFFERENT FitnessModifier values so the divisor and the squaring are both load-bearing, predict which parent GenePool.Next draws for each mutate slot, rather than reading the value back out of the system under test.")]
    public void Evolve_ScriptedRngDrivingGenePool_DrawsTheParentTheCumulativeSelectorArithmeticPredicts() {
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
            ["e0'2"] = 11.0f,
            ["e2'1"] = 20.0f,
            ["e2'2"] = 21.0f,
            ["e1'2"] = 22.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.5f, 0.85f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label),
                    Is.EquivalentTo(new[] { "e0", "e0'1", "e0'2" }),
                    "both mutate slots must draw e0 as predicted by the modifier-and-squaring-aware transform");
    }
}
