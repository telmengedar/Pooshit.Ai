using NightlyCode.Ai.Extensions;
using NightlyCode.Ai.Extern;
using NightlyCode.Ai.Net.Configurations;

namespace NightlyCode.Ai.Genetics;

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
    public Population(int size, Func<T> generator, Rng rng=null) {
        if (size <= 0)
            throw new ArgumentException("Size of population has to be a positive integer");
        rng ??= new();
        
        Entries = new PopulationEntry<T>[size];
        CrossSetup crossSetup = new() {
                                          MutateRange = 1.0f,
                                          Rng = rng
                                      };
        for (int i = 0; i < size; ++i) {
            Entries[i] = new() {
                                   Chromosome = generator()
                               };
            Entries[i].Chromosome.Randomize(crossSetup);
        }
    }

    /// <summary>
    /// creates a new <see cref="Population{T}"/>
    /// </summary>
    /// <param name="population">entries of population</param>
    public Population(PopulationEntry<T>[] population) {
        if (population.Length <= 0)
            throw new ArgumentException("Invalid population size");
        Entries = population;
    }

    /// <summary>
    /// entries in population
    /// </summary>
    public PopulationEntry<T>[] Entries { get; private set; }

    /// <summary>
    /// randomizes the full population
    /// </summary>
    /// <param name="rng">rng to use</param>
    void Randomize(Rng rng) {
        CrossSetup crossSetup = new() {
                                          MutateRange = 1.0f,
                                          Rng = rng
                                      };
        foreach (PopulationEntry<T> entry in Entries)
            entry.Chromosome.Randomize(crossSetup);
    }
    
    void Evolve(Rng rng, EvolutionSetup<T> setup) {
        if (Entries[0].Fitness < 0.0) {
            Randomize(rng);
            return;
        }
        
        PopulationEntry<T>[] next = new PopulationEntry<T>[Entries.Length];

        int elitismCount = (int)(Entries.Length * setup.Elitism);
        next[0] = Entries[0];
        int offset = 1;
        //float fitness = Entries[0].Fitness;
        int structure = Entries[0].Chromosome.StructureHash();
        for (int i = 1; i < Entries.Length; ++i) {
            int rhsStructure = Entries[i].Chromosome.StructureHash();
            if (structure == rhsStructure)
                continue;

            if (Entries[i].Fitness < 0.0f /*|| Entries[i].Fitness<=fitness*/)
                continue;


            structure = rhsStructure;
            next[offset++] = Entries[i];
            //fitness = Entries[i].Fitness;
            if (offset >= elitismCount || i >= Entries.Length-1)
                break;
        }

        float max = Math.Min(Entries.Max(e => e.Fitness), setup.MaxFitness);
        float fitnessSum = 0.0f;
        foreach (PopulationEntry<T> entry in Entries) {
            if (entry.Fitness < 0.0)
                continue;

            if (entry.Fitness >= max)
                fitnessSum += 1.0f / max;
            else {
                float value = (max - entry.Fitness) / max;
                value *= value;
                fitnessSum += value;
            }
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
            for (int i = offset; i < Entries.Length; ++i) {
                double first = rng.NextDouble() * fitnessSum;

                PopulationEntry<T> firstChromosome = Entries.FirstOrDefault(e => e.Fitness >= first) ?? Entries[0];

                float mutationRate = setup.Mutation.Rate;
                float mutationRange = setup.Mutation.Range;
                
                float mutationScale = (float)i / Entries.Length;
                mutationRate *= mutationScale;
                mutationRange *= mutationScale;

                next[i] = new() {
                                    Chromosome = ((IMutatingChromosome<T>)firstChromosome.Chromosome).Mutate(new() {
                                                                                                                       MutateRate = mutationRate,
                                                                                                                       MutateRange = mutationRange,
                                                                                                                       Rng = rng
                                                                                                                   })
                                };
            }
        }
        else throw new NotImplementedException();

        Entries = next;
    }

    double GetOrderNumber(PopulationEntry<T> entry) {
        if (entry.Fitness < 0.0)
            return double.MaxValue;
        return entry.Fitness;
    }
    /// <summary>
    /// trains the population
    /// </summary>
    /// <param name="setup">configuration values used for evolution</param>
    /// <returns>best training result</returns>
    public Tuple<T, double> Train(EvolutionSetup<T> setup) {
        if (setup.Threads > 1)
            return TrainParallel(setup);
        
        Rng rng = new();
        for (int i = 0; i < setup.Runs; i++) {
            if (i > 0)
                Evolve(rng, setup);

            foreach (PopulationEntry<T> entry in Entries)
                entry.Fitness = setup.TrainingSet.Select(t => t(entry.Chromosome)).Fitness(setup.FitnessAggregate);
            Array.Sort(Entries, (x, y) => -GetOrderNumber(y).CompareTo(GetOrderNumber(x)));
            if(Entries[0].Fitness>=0.0 && Entries[0].Fitness<setup.TargetFitness)
                return new(Entries[0].Chromosome, Entries[0].Fitness);
            setup.AfterRun?.Invoke(i, Entries[0].Fitness);
        }
        
        return new(Entries[0].Chromosome, Entries[0].Fitness);
    }
    
    /// <summary>
    /// trains the population
    /// </summary>
    /// <param name="setup">configuration values used for evolution</param>
    /// <returns>best training result</returns>
    Tuple<T, double> TrainParallel(EvolutionSetup<T> setup) {
        Rng rng = new();
        for (int i = 0; i < setup.Runs; i++) {
            if (i > 0)
                Evolve(rng, setup);

            Parallel.ForEach(Entries, new() {
                                                MaxDegreeOfParallelism = setup.Threads
                                            },
                             entry => { entry.Fitness = setup.TrainingSet.Select(t => t(entry.Chromosome)).Fitness(setup.FitnessAggregate); });
            Array.Sort(Entries, (x, y) => -GetOrderNumber(y).CompareTo(GetOrderNumber(x)));
            if (Entries[0].Fitness >= 0.0 && Entries[0].Fitness < setup.TargetFitness)
                return new(Entries[0].Chromosome, Entries[0].Fitness);
            setup.AfterRun?.Invoke(i, Entries[0].Fitness);
        }

        return new(Entries[0].Chromosome, Entries[0].Fitness);
    }
}