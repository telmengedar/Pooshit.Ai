namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// one persisted (problem, seed) measurement inside a committed <see cref="Baseline"/> (design
/// #9072 §7). Deliberately narrower than <see cref="BenchmarkRunResult"/> - it carries only what
/// the report compares against, never <c>GenerationZeroBest</c>, which is self-relative and has
/// no meaning across runs
/// </summary>
public sealed class BaselineRunRecord {

    public BaselineRunRecord() { }

    public BaselineRunRecord(string problemName, long seed, float finalFitness, int generations, int? nonFiniteGenerations) {
        ProblemName = problemName;
        Seed = seed;
        FinalFitness = finalFitness;
        Generations = generations;
        NonFiniteGenerations = nonFiniteGenerations;
    }

    /// <summary>
    /// name of the <see cref="BenchmarkProblem"/> this record belongs to
    /// </summary>
    public string ProblemName { get; set; }

    /// <summary>
    /// seed the recorded run's <see cref="Pooshit.Ai.Extern.Rng"/> was constructed from
    /// </summary>
    public long Seed { get; set; }

    /// <summary>
    /// the full-set re-score recorded for this (problem, seed) pair
    /// </summary>
    public float FinalFitness { get; set; }

    /// <summary>
    /// generations executed for this (problem, seed) pair, as recorded
    /// </summary>
    public int Generations { get; set; }

    /// <summary>
    /// how many of <see cref="Generations"/> reported at least one entry with a non-finite fitness,
    /// as recorded (design #9511) - the persistence signal for #9060's discriminating question
    /// ("did a non-zero count occur, and did it persist"). <c>null</c> means this record predates
    /// non-finite-generation tracking: a fact about the baseline's age, not a measured zero, and
    /// must never be treated as one when comparing - see <see cref="BenchmarkReport"/>
    /// </summary>
    public int? NonFiniteGenerations { get; set; }
}
