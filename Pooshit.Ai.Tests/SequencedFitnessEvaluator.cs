using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// returns fitness values from a fixed script, one per call, regardless of which chromosome
/// is passed - the same record-validate-throw discipline <see cref="SequenceRng"/> already
/// applies to random draws, applied here to fitness instead of randomness. Used where a test's
/// concern is <see cref="Population{T}.Train"/>'s control flow (target-fitness early exit,
/// <see cref="EvolutionSetup{T}.AfterRun"/> invocation count) rather than which specific
/// chromosome earned which score - a concern <see cref="StubFitnessEvaluator{T}"/> cannot serve
/// without predicting every generated label across several generations.
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
class SequencedFitnessEvaluator<T> : IFitnessEvaluator<T>
where T : IChromosome<T> {
    readonly float[] script;
    int index;

    public SequencedFitnessEvaluator(params float[] script) => this.script = script;

    /// <summary>
    /// number of calls made to <see cref="EvaluateFitness"/>
    /// </summary>
    public int CallCount { get; private set; }

    public float EvaluateFitness(T chromosome, IRng rng, bool fullSet) {
        CallCount++;
        if (index >= script.Length)
            throw new NotSupportedException($"{nameof(SequencedFitnessEvaluator<T>)} was called more times ({CallCount}) than scripted ({script.Length} value(s) provided)");
        return script[index++];
    }
}
