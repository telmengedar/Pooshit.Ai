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
public sealed record BenchmarkRunResult(string ProblemName, long Seed, float FinalFitness, float GenerationZeroBest, int Generations);
