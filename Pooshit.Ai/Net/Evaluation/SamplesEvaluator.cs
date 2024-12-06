using System.Collections.Concurrent;
using Pooshit.Ai.Extensions;
using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net.Evaluation;
using Pooshit.Ai.Neurons;

namespace Pooshit.Ai.Net;

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
    /// input generator to execute for input neurons with generation data
    /// </summary>
    public Action<TNet, TChromosome> InputGenerator { get; set; }
    
    /// <inheritdoc />
    public float EvaluateFitness(TChromosome chromosome, IRng rng, bool fullSet) {
        if (!nets.TryPop(out TNet net)) {
            net = (TNet)Activator.CreateInstance(typeof(TNet), chromosome);
            if (net == null)
                throw new InvalidOperationException("Unable to create new neuronal net");
        }
        else net.Update(chromosome);

        TrainingSample[] sampleBase = SampleCount == 0 || fullSet ? samples : samples.Shuffle(rng).Take(SampleCount).ToArray();
        float result = sampleBase.Select(s => {
            if (s.InputArray != null)
                net.SetInputValues(s.InputArray);
            else {
                foreach (KeyValuePair<string, float> input in s.Inputs)
                    net[input.Key] = input.Value;
            }

            InputGenerator?.Invoke(net, chromosome);
            net.Compute();

            switch (EvaluationFunc) {
                default:
                case EvaluationFunc.DistancePercent:
                    return s.Outputs.Select(o => MathF.Abs(net[o.Key] - o.Value) / MathF.Max(MathF.Abs(o.Value), 1.0f)).Average();
                case EvaluationFunc.Distance:
                    return s.Outputs.Select(o => MathF.Abs(net[o.Key] - o.Value)).Average();
            }
        }).Fitness();
        nets.Push(net);
        return result;
    }
}