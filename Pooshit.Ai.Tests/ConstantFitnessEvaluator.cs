using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// returns a fixed fitness value regardless of which chromosome is passed - a constant oracle
/// on purpose (the same deliberate-ignore shape as <see cref="FakeNet"/>'s constant-zero output,
/// R3 in the test README). Used where a test's concern is orthogonal to which specific chromosome
/// is being scored (e.g. the mutation-run escalation schedule, which is driven purely by whether
/// the leader's StructureHash changed) and an exact per-label map (<see cref="StubFitnessEvaluator{T}"/>)
/// would require predicting an unbounded number of generated labels across many generations.
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
class ConstantFitnessEvaluator<T> : IFitnessEvaluator<T>
where T : IChromosome<T> {
    readonly float fitness;

    public ConstantFitnessEvaluator(float fitness) => this.fitness = fitness;

    public int CallCount { get; private set; }

    public float EvaluateFitness(T chromosome, IRng rng, bool fullSet) {
        CallCount++;
        return fitness;
    }
}
