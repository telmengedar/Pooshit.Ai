using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net.Evaluation;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class SamplesEvaluatorTests {

    static TrainingSample[] BuildSamples(params float[] values) {
        return values.Select(v => new TrainingSample(new float[] { v }, new Dictionary<string, float> { ["result"] = v })).ToArray();
    }


    static FakeChromosome CreateChromosome() {
        return new([new() { Name = "result", Index = 0 }]);
    }


    static SamplesEvaluator<FakeChromosome, FakeNet> CreateEvaluator(TrainingSample[] samples, int sampleCount) {
        return new(samples) {
            SampleCount = sampleCount,
            EvaluationFunc = EvaluationFunc.Distance
        };
    }


    [Test, Parallelizable]
    public void EvaluateFitness_KnownRngSequenceRevisitingDisplacedIndex_SumsExpectedSamples() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildSamples(10, 20, 30, 40, 50), 3);
        FakeChromosome chromosome = CreateChromosome();
        SequenceRng rng = new(3, 0, 0);

        float fitness = evaluator.EvaluateFitness(chromosome, rng, false);

        Assert.That(fitness, Is.EqualTo(100.0f));
    }


    [Test, Parallelizable]
    public void EvaluateFitness_SampleCountExceedsSetSize_ClampsToFullSetSum() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildSamples(10, 20, 30), 10);
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        float fitness = evaluator.EvaluateFitness(chromosome, rng, false);

        Assert.That(fitness, Is.EqualTo(60.0f));
    }


    [Test, Parallelizable]
    public void EvaluateFitness_FullSetIgnoresSampleCount_SumsEverySample() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildSamples(10, 20, 30, 40, 50), 1);
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        float fitness = evaluator.EvaluateFitness(chromosome, rng, true);

        Assert.That(fitness, Is.EqualTo(150.0f));
    }


    [Test, Parallelizable]
    public void EvaluateFitness_RepeatedPartialEvaluations_PreservesSampleCache() {
        TrainingSample[] samples = BuildSamples(Enumerable.Range(1, 30).Select(v => (float)v).ToArray());
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(samples, 5);
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        Assert.That(evaluator.EvaluateFitness(chromosome, rng, true), Is.EqualTo(465.0f));

        for (int i = 0; i < 500; i++)
            evaluator.EvaluateFitness(chromosome, rng, false);

        Assert.That(evaluator.EvaluateFitness(chromosome, rng, true), Is.EqualTo(465.0f));
    }


    [Test, Parallelizable]
    public void EvaluateFitness_ConcurrentPartialEvaluations_PreservesSampleCache() {
        TrainingSample[] samples = BuildSamples(Enumerable.Range(1, 30).Select(v => (float)v).ToArray());
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(samples, 5);
        FakeChromosome chromosome = CreateChromosome();
        LockedRng rng = new(1);

        Assert.That(evaluator.EvaluateFitness(chromosome, rng, true), Is.EqualTo(465.0f));

        Parallel.For(0, 2000, _ => evaluator.EvaluateFitness(chromosome, rng, false));

        Assert.That(evaluator.EvaluateFitness(chromosome, rng, true), Is.EqualTo(465.0f));
    }
}
