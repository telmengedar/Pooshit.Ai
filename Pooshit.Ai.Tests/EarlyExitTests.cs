using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class EarlyExitTests {

    [Test, Parallelizable]
    [Description("A single-entry population costs exactly one evaluator call per generation and zero rng draws; the scripted fitness sequence reaches TargetFitness on the 3rd Evolve() call, so AfterRun (which fires only for generations that did not reach target) must fire exactly twice before Train stops early.")]
    public void Train_FitnessReachesTargetBeforeRunsExhausted_StopsAtThatGeneration() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> entry = new() {
            Chromosome = new("e0", log, structureHash: 0, fitnessModifier: 1.0f),
            AncestryId = Guid.NewGuid()
        };
        Population<MutatingFakeChromosome> population = new([entry], r => new("fresh", log, fitnessModifier: 1.0f));

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

        Assert.That(afterRunInvocations, Is.EqualTo(2));
        Assert.That(evaluator.CallCount, Is.EqualTo(5), "training must stop instead of consuming the remaining 7 of the 10 configured generations");
    }
}
