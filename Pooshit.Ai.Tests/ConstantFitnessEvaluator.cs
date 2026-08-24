using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// returns a fixed fitness value regardless of which chromosome is passed - a constant oracle
/// on purpose (the same deliberate-ignore shape as <see cref="FakeNet"/>'s constant-zero output,
/// R3 in the test README).
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
class ConstantFitnessEvaluator<T> : IFitnessEvaluator<T>
where T : IChromosome<T> {
    readonly float fitness;

    public ConstantFitnessEvaluator(float fitness) => this.fitness = fitness;

    public float EvaluateFitness(T chromosome, IRng rng, bool fullSet) => fitness;
}
