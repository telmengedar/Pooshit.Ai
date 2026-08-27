namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// outcome of one (problem, seed) benchmark run. <see cref="GenerationZeroBest"/> exists only to
/// support invariant I2 (design #9072 §11.3) - it is never persisted to <see cref="Baseline"/>
/// </summary>
/// <param name="ProblemName">name of the <see cref="BenchmarkProblem"/> that produced this result</param>
/// <param name="Seed">seed the run's <see cref="Pooshit.Ai.Extern.Rng"/> was constructed from</param>
/// <param name="FinalFitness">the full-set re-score <see cref="Pooshit.Ai.Genetics.Population{T}.Train"/> returns</param>
/// <param name="GenerationZeroBest">the best fitness present in the population before any generation ran</param>
/// <param name="Generations">how many generations executed before the target fitness was reached or <c>Runs</c> was exhausted</param>
/// <param name="NonFiniteGenerations">
/// how many of <see cref="Generations"/> reported at least one entry with a non-finite fitness via
/// <see cref="Pooshit.Ai.Genetics.EvolutionSetup{T}.OnNonFiniteFitness"/> - the persistence signal
/// DiVoid #9511 asks for ("did a non-zero count occur, and did it persist"), never a raw sum of
/// per-generation counts, which would conflate how many entries were affected at once with how many
/// generations were affected. Backs invariant I3 and is persisted to <see cref="Baseline"/>
/// </param>
public sealed record BenchmarkRunResult(string ProblemName, long Seed, float FinalFitness, float GenerationZeroBest, int Generations, int NonFiniteGenerations);
