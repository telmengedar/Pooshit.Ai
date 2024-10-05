using System.Collections.Concurrent;
using NightlyCode.Ai.Extensions;
using NightlyCode.Ai.Extern;
using NightlyCode.Ai.Genetics;

namespace NightlyCode.Ai.Net.DynamicBinOp;

/// <summary>
/// evaluates a chromosome based on a training set
/// </summary>
/// <typeparam name="T">type of chromosome to evaluate</typeparam>
public class DboEvaluator<T> : IFitnessEvaluator<T> 
    where T : DynamicBinOpConfiguration, IChromosome<T> {
    readonly TrainingSample[] samples;
    readonly ConcurrentStack<DynamicBinOpNet> nets = [];

    /// <summary>
    /// creates a new <see cref="DboEvaluator{T}"/>
    /// </summary>
    /// <param name="samples">samples to evaluate</param>
    public DboEvaluator(TrainingSample[] samples) => this.samples = samples;

    /// <summary>
    /// number of random samples to check
    /// </summary>
    public int SampleCount { get; init; }

    /// <inheritdoc />
    public float EvaluateFitness(T chromosome, IRng rng, bool fullSet) {
        if (!nets.TryPop(out DynamicBinOpNet net))
            net = new(chromosome);
        else net.Update(chromosome);

        TrainingSample[] sampleBase = SampleCount == 0 || fullSet ? samples : samples.Shuffle(rng).Take(SampleCount).ToArray();
        float result = sampleBase.Select(s => {
                                             foreach (KeyValuePair<string, float> input in s.Inputs)
                                                 net[input.Key] = input.Value;
                                             net.Compute();
                                             return s.Outputs.Select(o => Math.Abs(net[o.Key] - o.Value)).Average();
                                         }).Average();
        nets.Push(net);
        return result;
    }
}