using System.Numerics;
using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net.Evaluation;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class SamplesEvaluatorTests {
    const int LargeSetSize = 20;
    const float LargeSetSum = 1048575.0f;

    static TrainingSample[] BuildPowersOfTwo(int count) {
        return Enumerable.Range(0, count)
                          .Select(i => (float)(1L << i))
                          .Select(v => new TrainingSample(new float[] { v }, new Dictionary<string, float> { ["result"] = v }))
                          .ToArray();
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
    public void EvaluateFitness_KnownRngSequenceRevisitingDisplacedIndex_SumsExpectedSamplesWithExpectedBounds() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildPowersOfTwo(5), 3);
        FakeChromosome chromosome = CreateChromosome();
        SequenceRng rng = new(3, 0, 0);

        float fitness = evaluator.EvaluateFitness(chromosome, rng, false);

        Assert.That(fitness, Is.EqualTo(25.0f));
        Assert.That(rng.Bounds, Is.EqualTo(new[] { 5, 4, 3 }));
    }


    [Test, Parallelizable]
    public void EvaluateFitness_SampleCountExceedsSetSize_ClampsToFullSetSum() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildPowersOfTwo(5), 10);
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        float fitness = evaluator.EvaluateFitness(chromosome, rng, false);

        Assert.That(fitness, Is.EqualTo(31.0f));
    }


    [Test, Parallelizable]
    public void EvaluateFitness_FullSetIgnoresSampleCount_SumsEverySample() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildPowersOfTwo(5), 1);
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        float fitness = evaluator.EvaluateFitness(chromosome, rng, true);

        Assert.That(fitness, Is.EqualTo(31.0f));
    }


    [Test, Parallelizable]
    public void EvaluateFitness_NegativeSampleCount_ReturnsZeroFitness() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildPowersOfTwo(5), -1);
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        float fitness = evaluator.EvaluateFitness(chromosome, rng, false);

        Assert.That(fitness, Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    public void EvaluateFitness_PartialEvaluations_DrawDistinctSamples() {
        TrainingSample[] samples = BuildPowersOfTwo(LargeSetSize);
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(samples, 5);
        FakeChromosome chromosome = CreateChromosome();

        for (int seed = 1; seed <= 20; seed++) {
            Rng rng = new(seed);
            float fitness = evaluator.EvaluateFitness(chromosome, rng, false);
            Assert.That(BitOperations.PopCount((uint)fitness), Is.EqualTo(5));
        }
    }


    [Test, Parallelizable]
    public void EvaluateFitness_RepeatedPartialEvaluations_PreservesSampleCache() {
        TrainingSample[] samples = BuildPowersOfTwo(LargeSetSize);
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(samples, 5);
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        Assert.That(evaluator.EvaluateFitness(chromosome, rng, true), Is.EqualTo(LargeSetSum));

        for (int i = 0; i < 500; i++)
            evaluator.EvaluateFitness(chromosome, rng, false);

        Assert.That(evaluator.EvaluateFitness(chromosome, rng, true), Is.EqualTo(LargeSetSum));
    }


    [Test, Parallelizable]
    public void EvaluateFitness_ConcurrentPartialEvaluations_PreservesSampleCache() {
        TrainingSample[] samples = BuildPowersOfTwo(LargeSetSize);
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(samples, 5);
        FakeChromosome chromosome = CreateChromosome();
        LockedRng rng = new(1);

        Assert.That(evaluator.EvaluateFitness(chromosome, rng, true), Is.EqualTo(LargeSetSum));

        Parallel.For(0, 2000, _ => evaluator.EvaluateFitness(chromosome, rng, false));

        Assert.That(evaluator.EvaluateFitness(chromosome, rng, true), Is.EqualTo(LargeSetSum));
    }
}
