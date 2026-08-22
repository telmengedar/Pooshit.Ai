using System.Collections.Concurrent;
using System.Reflection;
using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Net.Evaluation;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

class FakeChromosome : IChromosome<FakeChromosome> {
    public FakeChromosome(NeuronConfig[] neurons) => Neurons = neurons;

    public void Randomize(CrossSetup setup = null) { }
    public int StructureHash() => 0;
    public float FitnessModifier => 0.0f;
    public FakeChromosome Optimize(Func<FakeChromosome, bool> test) => this;
    public NeuronConfig[] Neurons { get; }
}

class FakeNet : INeuronalNet<FakeChromosome> {
    readonly Dictionary<int, float> values = new();

    public FakeNet(FakeChromosome chromosome) { }

    public int InputWrites { get; private set; }

    public float this[string name] {
        get => 0.0f;
        set { }
    }

    public float this[int index] {
        get => values.GetValueOrDefault(index);
        set => values[index] = value;
    }

    public void Compute() { }

    public void SetInputValues(float[] inputValues) => InputWrites++;

    public void Update(FakeChromosome configuration) { }
}

[TestFixture, Parallelizable]
public class SamplesEvaluatorTests {
    const int SampleSetSize = 30;
    const int SubsetSize = 5;

    static TrainingSample[] BuildDistinctSamples() {
        TrainingSample[] samples = new TrainingSample[SampleSetSize];
        for (int i = 0; i < SampleSetSize; i++)
            samples[i] = new(new float[] { i }, new Dictionary<string, float> { ["result"] = i });
        return samples;
    }

    static SamplesEvaluator<FakeChromosome, FakeNet> CreateEvaluator(TrainingSample[] samples) {
        return new(samples) {
            SampleCount = SubsetSize
        };
    }

    static FakeChromosome CreateChromosome() {
        return new([new() { Name = "result", Index = 0 }]);
    }

    static float[] ReadCachedOutputValues(SamplesEvaluator<FakeChromosome, FakeNet> evaluator) {
        FieldInfo field = typeof(SamplesEvaluator<FakeChromosome, FakeNet>).GetField("indexedSamples", BindingFlags.NonPublic | BindingFlags.Instance);
        IndexedTrainingSample[] cached = (IndexedTrainingSample[])field!.GetValue(evaluator);
        return cached.Select(s => s.Outputs[0].Value).OrderBy(v => v).ToArray();
    }

    static FakeNet ReadPooledNet(SamplesEvaluator<FakeChromosome, FakeNet> evaluator) {
        FieldInfo field = typeof(SamplesEvaluator<FakeChromosome, FakeNet>).GetField("nets", BindingFlags.NonPublic | BindingFlags.Instance);
        ConcurrentStack<FakeNet> pool = (ConcurrentStack<FakeNet>)field!.GetValue(evaluator);
        pool.TryPeek(out FakeNet net);
        return net;
    }

    [Test, Parallelizable]
    public void EvaluateFitness_RepeatedPartialEvaluations_PreservesSampleCache() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildDistinctSamples());
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        for (int i = 0; i < 500; i++)
            evaluator.EvaluateFitness(chromosome, rng, false);

        Assert.That(ReadCachedOutputValues(evaluator), Is.EqualTo(Enumerable.Range(0, SampleSetSize).Select(v => (float)v).ToArray()));
    }

    [Test, Parallelizable]
    public void EvaluateFitness_ConcurrentPartialEvaluations_PreservesSampleCache() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildDistinctSamples());
        FakeChromosome chromosome = CreateChromosome();
        LockedRng rng = new(1);

        Parallel.For(0, 2000, _ => evaluator.EvaluateFitness(chromosome, rng, false));

        Assert.That(ReadCachedOutputValues(evaluator), Is.EqualTo(Enumerable.Range(0, SampleSetSize).Select(v => (float)v).ToArray()));
    }

    [Test, Parallelizable]
    public void EvaluateFitness_PartialEvaluation_TouchesExactlySampleCountSamples() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildDistinctSamples());
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        evaluator.EvaluateFitness(chromosome, rng, false);

        Assert.That(ReadPooledNet(evaluator).InputWrites, Is.EqualTo(SubsetSize));
    }

    [Test, Parallelizable]
    public void EvaluateFitness_FullSetEvaluation_TouchesEverySample() {
        SamplesEvaluator<FakeChromosome, FakeNet> evaluator = CreateEvaluator(BuildDistinctSamples());
        FakeChromosome chromosome = CreateChromosome();
        Rng rng = new(1);

        evaluator.EvaluateFitness(chromosome, rng, true);

        Assert.That(ReadPooledNet(evaluator).InputWrites, Is.EqualTo(SampleSetSize));
    }
}
