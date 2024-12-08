using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics.Mutation;
using Pooshit.Ai.Net;

namespace Pooshit.Ai.Genetics;

/// <summary>
/// population of <see cref="IChromosome{T}"/>
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
public class Population<T> 
where T : class, IChromosome<T> {

    /// <summary>
    /// creates a new <see cref="Population{T}"/>
    /// </summary>
    /// <param name="size">size of population</param>
    /// <param name="generator">used to generate new chromosomes</param>
    /// <param name="rng">rng to use to initialize population</param>
    public Population(int size, Func<IRng, T> generator, Rng rng=null) {
        if (size <= 0) 
            throw new ArgumentException("Size of population has to be a positive integer");
        Generator = generator;
        rng ??= new();
        
        Entries = new PopulationEntry<T>[size];
        CrossSetup crossSetup = new() {
                                          MutateRange = 1.0f,
                                          Rng = rng
                                      };
        for (int i = 0; i < size; ++i) {
            Entries[i] = new() {
                                   Chromosome = generator(rng)
                               };
            Entries[i].Chromosome.Randomize(crossSetup);
        }
    }

    /// <summary>
    /// creates a new <see cref="Population{T}"/>
    /// </summary>
    /// <param name="population">entries of population</param>
    /// <param name="generator">generator for new chromosome instances</param>
    public Population(PopulationEntry<T>[] population, Func<IRng, T> generator) {
        if (population.Length <= 0)
            throw new ArgumentException("Invalid population size");
        Generator = generator;
        Entries = population;
    }

    /// <summary>
    /// generates a new clean configuration
    /// </summary>
    public Func<IRng, T> Generator { get; }

    /// <summary>
    /// entries in population
    /// </summary>
    public PopulationEntry<T>[] Entries { get; private set; }
    
    void Evolve(IRng rng, EvolutionSetup<T> setup) {
        PopulationEntry<T>[] next = new PopulationEntry<T>[Entries.Length];

        int elitismCount = (int)(Entries.Length * setup.Elitism);
        
        HashSet<int> structureHashes = [];

        int offset = 0;
        foreach (PopulationEntry<T> entry in Entries) {
            if (entry.Fitness < 0.0f)
                continue;

            int structureHash = entry.Chromosome.StructureHash();
            if (!structureHashes.Add(structureHash))
                continue;

            next[offset++] = entry;
            if (offset >= elitismCount)
                break;
        }

        float modifiedMax = Entries.Max(e => (e.Fitness + 1.0f) / e.Chromosome.FitnessModifier);
        
        float fitnessSum = 0.0f;
        foreach (PopulationEntry<T> entry in Entries) {
            if (entry.Fitness < 0.0)
                continue;

            float value = (modifiedMax - (entry.Fitness + 1.0f) / entry.Chromosome.FitnessModifier) / modifiedMax;

            value *= value;
            fitnessSum += value;
            entry.Fitness = fitnessSum;
        }

        if (typeof(ICrossChromosome<T>).IsAssignableFrom(typeof(T))) {
            for (int i = offset; i < Entries.Length; ++i) {
                double first = rng.NextDouble() * fitnessSum;
                double second = rng.NextDouble() * fitnessSum;

                PopulationEntry<T> firstChromosome = Entries.FirstOrDefault(e => e.Fitness >= first) ?? Entries[0];
                PopulationEntry<T> secondChromosome = Entries.FirstOrDefault(e => e.Fitness >= second) ?? Entries[0];

                float mutationChance = setup.Mutation.Chance;
                float mutationRate = setup.Mutation.Rate;
                float mutationRange = setup.Mutation.Range;

                //if (firstChromosome.Chromosome.GenerateHash() == secondChromosome.Chromosome.GenerateHash())
                if (Math.Abs(firstChromosome.Fitness - secondChromosome.Fitness) <= double.Epsilon * 2.0) {
                    // scale mutation up when crossing a chromosome with itself
                    mutationChance = 1.0f;
                    mutationRate *= setup.Mutation.IncestFactor;
                    mutationRange *= setup.Mutation.IncestFactor;
                }
                else if (i >= Entries.Length - elitismCount * 2)
                    mutationChance = 0.15f + (float)(Entries.Length - i) / (elitismCount * 2);

                float mutationScale = (float)i / Entries.Length;
                mutationRate *= mutationScale;
                mutationRange *= mutationScale;

                next[i] = new() {
                                    Chromosome = ((ICrossChromosome<T>)firstChromosome.Chromosome).Cross(secondChromosome.Chromosome, new() {
                                                                                                                                                MutateChance = mutationChance,
                                                                                                                                                MutateRate = mutationRate,
                                                                                                                                                MutateRange = mutationRange,
                                                                                                                                                Rng = rng
                                                                                                                                            })
                                };
            }
        }
        else if (typeof(IMutatingChromosome<T>).IsAssignableFrom(typeof(T))) {
            if (setup.Threads > 1) {
                Parallel.For(offset,
                             Entries.Length,
                             new() {
                                       MaxDegreeOfParallelism = setup.Threads
                                   },
                             i => {
                                 if (Generator != null && i > Entries.Length - elitismCount) {
                                     T chromosome = Generator(rng);
                                     Mutate(next, rng, chromosome, setup.Mutation, i);
                                 }
                                 else Mutate(next, rng, fitnessSum, setup.Mutation, i);
                             });
            }
            else {
                for (int i = offset; i < Entries.Length; ++i) {
                    if (Generator != null && i > Entries.Length - elitismCount) {
                        T chromosome = Generator(rng);
                        Mutate(next, rng, chromosome, setup.Mutation, i);
                    }
                    else Mutate(next, rng, fitnessSum, setup.Mutation, i);
                }
            }
        }
        else throw new NotImplementedException();

        Entries = next;
    }
    
    void Mutate(PopulationEntry<T>[] next, IRng rng, float fitnessSum, MutationSetup setup, int i) {
        float first = rng.NextFloat() * fitnessSum;
        PopulationEntry<T> firstChromosome = Entries.FirstOrDefault(e => e.Fitness >= first) ?? Entries[rng.NextInt(Entries.Length)];
        T chromosome = firstChromosome.Chromosome;
        Mutate(next, rng, chromosome, setup, i);
    }
    
    void Mutate(PopulationEntry<T>[] next, IRng rng, T chromosome, MutationSetup setup, int i) {
        float mutationRange = setup.Range;

        float mutationScale = (float)i / Entries.Length;
        mutationRange *= mutationScale;

        int runs = 1 + rng.NextInt(setup.Runs);
        for (int k = 0; k < runs; ++k)
            chromosome = ((IMutatingChromosome<T>)chromosome).Mutate(rng, mutationRange);

        next[i] = new() {
            Chromosome = chromosome
        };
    }

    float GetOrderNumber(PopulationEntry<T> entry) {
        if (entry.Fitness < 0.0)
            return float.MaxValue;
        return entry.Fitness;
    }

    void EvaluateFitness(EvolutionSetup<T> setup, IRng rng, bool fullSet) {
        foreach (PopulationEntry<T> entry in Entries)
            entry.Fitness = setup.Evaluator.EvaluateFitness(entry.Chromosome, rng, fullSet);
        Array.Sort(Entries, (x, y) => -GetOrderNumber(y).CompareTo(GetOrderNumber(x)));
    }

    void EvaluateParallel(EvolutionSetup<T> setup, IRng rng, bool fullSet) {
        Parallel.ForEach(Entries, new() {
                                            MaxDegreeOfParallelism = setup.Threads
                                        },
                         entry => {
                             entry.Fitness = setup.Evaluator.EvaluateFitness(entry.Chromosome, rng, fullSet);
                         });
        Array.Sort(Entries, (x, y) => -GetOrderNumber(y).CompareTo(GetOrderNumber(x)));
    }
    
    /// <summary>
    /// trains the population
    /// </summary>
    /// <param name="setup">configuration values used for evolution</param>
    /// <returns>best training result</returns>
    public PopulationEntry<T> Train(EvolutionSetup<T> setup) {
        IRng rng = setup.Threads > 1 ? new LockedRng() : new Rng();
        Action<EvolutionSetup<T>, IRng, bool> evaluation = setup.Threads > 1 ? EvaluateParallel : EvaluateFitness;
        evaluation(setup, rng, true);
        int bestStructure = Entries[0].Chromosome.StructureHash();
        int bestRun = 0;

        for (int i = 0; i < setup.Runs; i++) {
            Evolve(rng, setup);

            evaluation(setup, rng, false);
            if (Entries[0].Fitness <= setup.TargetFitness)
                break;

            int structure = Entries[0].Chromosome.StructureHash();
            if (structure == bestStructure)
                setup.Mutation.Runs = Math.Min(50, 1 + ((i - bestRun) >> 6) * 5);
            else {
                setup.Mutation.Runs = 1;
                bestStructure = structure;
                bestRun = i;
            }

            setup.AfterRun?.Invoke(i, Entries[0].Fitness);
        }

        evaluation(setup, rng, true);

        return Entries[0];
    }
}