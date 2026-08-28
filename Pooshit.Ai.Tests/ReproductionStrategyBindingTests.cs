using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class ReproductionStrategyBindingTests {

    [Test, Parallelizable]
    public void Constructor_ChromosomeImplementsOnlyIMutatingChromosome_BindsMutateStrategy() {
        List<MutateCall> mutateLog = [];
        int counter = 0;
        Population<MutatingFakeChromosome> population = new(2, r => new($"gen{counter++}", mutateLog, fitnessModifier: 1.0f));
        Dictionary<string, float> fitnessByLabel = new() {
            ["gen0"] = 1.0f,
            ["gen1"] = 2.0f,
            ["gen0'1"] = 3.0f,
            ["gen0'2"] = 4.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.0f, 0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 0,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(mutateLog, Is.Not.Empty);
    }


    [Test, Parallelizable]
    public void Constructor_ChromosomeImplementsOnlyICrossChromosome_BindsCrossStrategy() {
        List<CrossCall> crossLog = [];
        int counter = 0;
        Population<CrossingFakeChromosome> population = new(2, r => new($"gen{counter++}", crossLog, fitnessModifier: 1.0f));
        Dictionary<string, float> fitnessByLabel = new() {
            ["gen0"] = 1.0f,
            ["gen1"] = 2.0f,
            ["gen0xgen0"] = 3.0f
        };
        StubFitnessEvaluator<CrossingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<CrossingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng { FloatValues = [0.0f, 0.0f, 0.0f, 0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 0,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(crossLog, Is.Not.Empty);
    }


    [Test, Parallelizable]
    public void Constructor_ChromosomeImplementsBothInterfaces_BindsCrossAndNeverMutates() {
        List<string> mutateLog = [];
        List<string> crossLog = [];
        int counter = 0;
        Population<AmbidextrousFakeChromosome> population = new(2, r => new($"gen{counter++}", mutateLog, crossLog, fitnessModifier: 1.0f));
        Dictionary<string, float> fitnessByLabel = new() {
            ["gen0"] = 1.0f,
            ["gen1"] = 2.0f,
            ["gen0xgen0"] = 3.0f
        };
        StubFitnessEvaluator<AmbidextrousFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<AmbidextrousFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng { FloatValues = [0.0f, 0.0f, 0.0f, 0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 0,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(crossLog, Is.Not.Empty);
        Assert.That(mutateLog, Is.Empty);
    }


    [Test, Parallelizable]
    public void Constructor_ChromosomeImplementsNeitherInterface_ThrowsAtConstruction() {
        Assert.That(() => new Population<FakeChromosome>(2, r => new([], "gen")),
                    Throws.InstanceOf<NotImplementedException>());
    }
}
