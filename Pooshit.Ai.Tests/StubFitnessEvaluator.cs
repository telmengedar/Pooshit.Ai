using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// returns a caller-specified fitness per chromosome label instead of computing one.
/// inverts the usual problem: fitness becomes an input the test controls, so selection,
/// elitism and eviction can be observed against known values instead of inferred from a
/// distribution (design #9072 §8.2)
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
class StubFitnessEvaluator<T> : IFitnessEvaluator<T>
where T : IChromosome<T>, ILabelledChromosome {
    readonly IReadOnlyDictionary<string, float> fitnessByLabel;

    public StubFitnessEvaluator(IReadOnlyDictionary<string, float> fitnessByLabel) => this.fitnessByLabel = fitnessByLabel;

    /// <summary>
    /// ordered log of every call made to <see cref="EvaluateFitness"/>
    /// </summary>
    public List<(string Label, bool FullSet)> Calls { get; } = new();

    public float EvaluateFitness(T chromosome, IRng rng, bool fullSet) {
        Calls.Add((chromosome.Label, fullSet));
        if (!fitnessByLabel.TryGetValue(chromosome.Label, out float fitness))
            throw new KeyNotFoundException($"{nameof(StubFitnessEvaluator<T>)} was not given a fitness for chromosome labelled '{chromosome.Label}'");
        return fitness;
    }
}
