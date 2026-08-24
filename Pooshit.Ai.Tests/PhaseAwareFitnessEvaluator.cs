using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// scores a chromosome from a caller-supplied function of (StructureHash, Phase), with
/// <see cref="Phase"/> mutable from outside the evaluation loop (e.g. from
/// <see cref="EvolutionSetup{T}.AfterRun"/>). Used where a test needs the population's leader
/// to change identity at a chosen generation without predicting the unbounded set of labels a
/// multi-generation run produces - a concern neither <see cref="StubFitnessEvaluator{T}"/>
/// (label-keyed) nor <see cref="ConstantFitnessEvaluator{T}"/> (phase-blind) can serve.
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
class PhaseAwareFitnessEvaluator<T> : IFitnessEvaluator<T>
where T : IChromosome<T> {
    readonly Func<int, int, float> scorer;

    public PhaseAwareFitnessEvaluator(Func<int, int, float> scorer) => this.scorer = scorer;

    public int Phase { get; set; }

    public float EvaluateFitness(T chromosome, IRng rng, bool fullSet) => scorer(chromosome.StructureHash(), Phase);
}
