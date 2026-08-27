using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Net.Evaluation;

namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// one named, self-contained training problem the benchmark harness measures
/// </summary>
public abstract class BenchmarkProblem {

    protected BenchmarkProblem(string name, float targetFitness) {
        Name = name;
        TargetFitness = targetFitness;
    }

    /// <summary>
    /// stable name of the problem. Baseline records key on this - a problem whose samples change
    /// must take a new name (design #9072 §7)
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// fitness threshold below which the problem counts as solved for reporting purposes
    /// </summary>
    public float TargetFitness { get; }

    /// <summary>
    /// runs the problem once against the given seed and returns the outcome
    /// </summary>
    /// <param name="seed">non-zero seed the run's <see cref="Rng"/> is constructed from</param>
    public abstract BenchmarkRunResult Run(long seed);
}

/// <summary>
/// a <see cref="BenchmarkProblem"/> for a concrete chromosome/net family. Every <see cref="Run"/>
/// call constructs a fresh <see cref="Genetics.EvolutionSetup{T}"/>, a fresh
/// <see cref="SamplesEvaluator{TChromosome,TNet}"/> and a fresh <see cref="Population{T}"/> -
/// non-optional, see design #9072 §10.2 H7/H8
/// </summary>
/// <typeparam name="TChromosome">type of chromosome trained</typeparam>
/// <typeparam name="TNet">type of neuronal net the chromosome configures</typeparam>
public sealed class BenchmarkProblem<TChromosome, TNet> : BenchmarkProblem
where TChromosome : class, IChromosome<TChromosome>
where TNet : INeuronalNet<TChromosome> {
    readonly int populationSize;
    readonly Func<IRng, TChromosome> generator;
    readonly Func<TrainingSample[]> samples;
    readonly int runs;
    readonly int rivalism;

    public BenchmarkProblem(string name, int populationSize, Func<IRng, TChromosome> generator, Func<TrainingSample[]> samples, int runs, int rivalism, float targetFitness)
    : base(name, targetFitness) {
        this.populationSize = populationSize;
        this.generator = generator;
        this.samples = samples;
        this.runs = runs;
        this.rivalism = rivalism;
    }

    /// <inheritdoc />
    public override BenchmarkRunResult Run(long seed) {
        Rng rng = new(seed);
        Population<TChromosome> population = new(populationSize, generator, rng);
        SamplesEvaluator<TChromosome, TNet> evaluator = new(samples());

        float generationZeroBest = population.Entries.Min(entry => evaluator.EvaluateFitness(entry.Chromosome, rng, true));

        int generationsExecuted = 0;
        EvolutionSetup<TChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = rng,
            Threads = 1,
            Runs = runs,
            Rivalism = rivalism,
            TargetFitness = TargetFitness,
            AfterRun = (generation, _) => generationsExecuted = generation + 1
        };

        PopulationEntry<TChromosome> result = population.Train(setup);
        if (result.Fitness <= TargetFitness)
            generationsExecuted++;

        return new BenchmarkRunResult(Name, seed, result.Fitness, generationZeroBest, generationsExecuted);
    }
}
