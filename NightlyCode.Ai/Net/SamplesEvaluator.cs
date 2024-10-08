using System.Collections.Concurrent;
using NightlyCode.Ai.Extensions;
using NightlyCode.Ai.Extern;
using NightlyCode.Ai.Genetics;

namespace NightlyCode.Ai.Net;

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
                                             if(s.InputArray!=null)
                                                 net.SetInputValues(s.InputArray);
                                             else {
                                                 foreach (KeyValuePair<string, float> input in s.Inputs)
                                                     net[input.Key] = input.Value;
                                             }

                                             net.Compute();
                                             return s.Outputs.Select(o => Math.Abs(net[o.Key] - o.Value)).Average();
                                         }).Average();
        nets.Push(net);
        return result;
    }
}