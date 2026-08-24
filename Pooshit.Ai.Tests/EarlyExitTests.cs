using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class EarlyExitTests {

    [Test, Parallelizable]
    public void Train_FitnessReachesTargetBeforeRunsExhausted_StopsAtThatGeneration() {
        List<MutateCall> log = [];
        // a single-entry population never has a mutate slot to fill (elitism always claims
        // the only entry), so every generation costs exactly one evaluator call and zero rng draws
        PopulationEntry<MutatingFakeChromosome> entry = new() {
            Chromosome = new("e0", log, structureHash: 0, fitnessModifier: 1.0f),
            AncestryId = Guid.NewGuid()
        };
        Population<MutatingFakeChromosome> population = new([entry], r => new("fresh", log, fitnessModifier: 1.0f));

        // [initial fullSet score, gen0, gen1, gen2 (<= target, triggers the stop), final fullSet re-score]
        SequencedFitnessEvaluator<MutatingFakeChromosome> evaluator = new(100.0f, 50.0f, 30.0f, 0.1f, 0.05f);
        int afterRunInvocations = 0;
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(),
            Threads = 1,
            Runs = 10,
            Elitism = 1,
            TargetFitness = 1.0f,
            AfterRun = (generation, fitness) => afterRunInvocations++
        };

        population.Train(setup);

        // reached target on the 3rd Evolve() call (generation index 2); AfterRun is only invoked
        // for generations that did NOT reach target, so it must fire exactly twice (i = 0, 1)
        Assert.That(afterRunInvocations, Is.EqualTo(2));
        Assert.That(evaluator.CallCount, Is.EqualTo(5), "training must stop instead of consuming the remaining 7 of the 10 configured generations");
    }
}
