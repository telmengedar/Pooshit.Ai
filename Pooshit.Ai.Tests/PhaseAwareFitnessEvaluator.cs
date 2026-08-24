using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// scores a chromosome from a caller-supplied function of (StructureHash, Phase), with
/// <see cref="Phase"/> mutable from outside the evaluation loop (e.g. from
/// <see cref="EvolutionSetup{T}.AfterRun"/>). Used where a test needs the population's leader
/// to change identity at a chosen generation without predicting the unbounded set of labels a
/// multi-generation run produces - a concern <see cref="StubFitnessEvaluator{T}"/> (label-keyed)
/// cannot serve.
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
class PhaseAwareFitnessEvaluator<T> : IFitnessEvaluator<T>
where T : IChromosome<T> {
    readonly Func<int, int, float> scorer;

    public PhaseAwareFitnessEvaluator(Func<int, int, float> scorer) => this.scorer = scorer;

    public int Phase { get; set; }

    /// <summary>
    /// ordered log of (StructureHash, Phase, fullSet) for every call
    /// </summary>
    public List<(int StructureHash, int Phase, bool FullSet)> Calls { get; } = new();

    public float EvaluateFitness(T chromosome, IRng rng, bool fullSet) {
        Calls.Add((chromosome.StructureHash(), Phase, fullSet));
        return scorer(chromosome.StructureHash(), Phase);
    }
}
