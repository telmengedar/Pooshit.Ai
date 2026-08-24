using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class BreedingWeightTests {

    /// <summary>
    /// drives a real generation through <see cref="Population{T}.Train"/> with hand-chosen fitness
    /// values and a scripted <see cref="IRng"/>, and asserts the parent drawn for each mutate slot
    /// is the one Population's private breeding-weight transform plus <see cref="GenePool{T}.Next"/>'s
    /// cumulative-selector arithmetic predicts - computed independently here (R6, design #9072 §10.1)
    /// rather than by reading the value back out of the system under test.
    /// </summary>
    [Test, Parallelizable]
    public void Evolve_ScriptedRngDrivingGenePool_DrawsTheParentTheCumulativeSelectorArithmeticPredicts() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = new() { Chromosome = new("e0", log, 0, 1.0f), Fitness = 0.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e1 = new() { Chromosome = new("e1", log, 1, 1.0f), Fitness = 1.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e2 = new() { Chromosome = new("e2", log, 2, 1.0f), Fitness = 3.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        // independent oracle - Population.Evolve's private breeding-weight transform, computed by
        // hand from the fitness/FitnessModifier values above, not read out of the system under test:
        //   modifiedMax = max((fitness + 1) / modifier) = max(1, 2, 4) = 4
        //   weight(e) = ((modifiedMax - (fitness + 1) / modifier) / modifiedMax) ^ 2
        //   weight(e0) = ((4 - 1) / 4)^2 = 0.5625      cumulative selector: 0.5625
        //   weight(e1) = ((4 - 2) / 4)^2 = 0.25         cumulative selector: 0.8125
        //   weight(e2) = ((4 - 4) / 4)^2 = 0.0          cumulative selector: 0.8125 (unchanged)
        //   fitnessSum = 0.8125
        // GenePool.Next: selectorValue = NextFloat() * fitnessSum; first entry whose cumulative
        // selector >= selectorValue wins.
        //   NextFloat = 0.9 -> selectorValue = 0.9 * 0.8125 = 0.73125 -> e0 (0.5625) too low, e1 (0.8125) wins -> e1
        //   NextFloat = 0.5 -> selectorValue = 0.5 * 0.8125 = 0.40625 -> e0 (0.5625) already satisfies -> e0
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
