using System.Collections.Concurrent;
using Pooshit.Ai.Extensions;
using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net.Operations;
using Pooshit.Ai.Neurons;

namespace Pooshit.Ai.Net.Evaluation;

/// <summary>
/// evaluates a chromosome based on a training set
/// </summary>
/// <typeparam name="TChromosome">type of chromosome to evaluate</typeparam>
/// <typeparam name="TNet">type of neuronal net to use for testing</typeparam>
/// <remarks>
/// <see cref="TNet"/> needs to provide a constructor with one parameter of type <see cref="TChromosome"/>
/// for this evaluator to work
/// </remarks>
public class SamplesEvaluator<TChromosome, TNet> : IFitnessEvaluator<TChromosome> 
    where TChromosome : IChromosome<TChromosome> 
    where TNet : INeuronalNet<TChromosome> {
    readonly TrainingSample[] samples;
    IndexedTrainingSample[] indexedSamples;
    readonly ConcurrentStack<TNet> nets = [];

    /// <summary>
    /// creates a new <see cref="SamplesEvaluator{TChromosome,TNet}"/>
    /// </summary>
    /// <param name="samples">samples to evaluate</param>
    public SamplesEvaluator(TrainingSample[] samples) => this.samples = samples;

    /// <summary>
    /// number of random samples to check
    /// </summary>
    public int SampleCount { get; init; }

    /// <summary>
    /// function used to evaluate chromosome fitness
    /// </summary>
    public EvaluationFunc EvaluationFunc { get; set; } = EvaluationFunc.DistancePercent;

    /// <summary>
    /// aggregate func used for fitness values
    /// </summary>
    public AggregateType FitnessAggregate { get; set; } = AggregateType.Sum;
    
    /// <summary>
    /// input generator to execute for input neurons with generation data
    /// </summary>
    public Action<TNet, TChromosome> InputGenerator { get; set; }

    IEnumerable<IndexedTrainingSample> TranslateSamples(TChromosome chromosome) {
        foreach (TrainingSample sample in samples) {
            yield return new() {
                InputArray = sample.InputArray,
                Inputs = sample.Inputs?.Select(i => new NeuronValue {
                    Index = chromosome.Neurons.FirstOrDefault(n => n.Name == i.Key)?.Index ?? -1,
                    Value = i.Value
                }).ToArray(),
                Outputs = sample.Outputs.Select(i => new NeuronValue {
                    Index = chromosome.Neurons.FirstOrDefault(n => n.Name == i.Key)?.Index ?? -1,
                    Value = i.Value
                }).ToArray()
            };
        }
    }
    
    /// <inheritdoc />
    public float EvaluateFitness(TChromosome chromosome, IRng rng, bool fullSet) {
        if (!nets.TryPop(out TNet net)) {
            net = (TNet)Activator.CreateInstance(typeof(TNet), chromosome);
            if (net == null)
                throw new InvalidOperationException("Unable to create new neuronal net");
        }
        else net.Update(chromosome);

        indexedSamples ??= TranslateSamples(chromosome).ToArray();

        IndexedTrainingSample[] sampleBase = SampleCount == 0 || fullSet ? indexedSamples : indexedSamples.Shuffle(rng).Take(SampleCount).ToArray();
        float result = sampleBase.Select(s => {
            if (s.InputArray != null)
                net.SetInputValues(s.InputArray);
            else {
                foreach (NeuronValue input in s.Inputs)
                    net[input.Index] = input.Value;
            }

            InputGenerator?.Invoke(net, chromosome);
            net.Compute();

            switch (EvaluationFunc) {
                default:
                case EvaluationFunc.DistancePercent:
                    return s.Outputs.Select(o => MathF.Abs(net[o.Index] - o.Value) / MathF.Max(MathF.Abs(o.Value), 1.0f)).Average();
                case EvaluationFunc.Distance:
                    return s.Outputs.Select(o => MathF.Abs(net[o.Index] - o.Value)).Average();
            }
        }).Aggregate(FitnessAggregate);
        nets.Push(net);
        return result;
    }
}