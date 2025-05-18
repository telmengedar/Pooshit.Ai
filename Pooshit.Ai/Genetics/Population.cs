using Pooshit.Ai.Extern;
using Pooshit.Ai.Net;

namespace Pooshit.Ai.Genetics;

/// <summary>
/// population of <see cref="IChromosome{T}"/>
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
public class Population<T> 
where T : class, IChromosome<T> {
    PopulationEntry<T>[] trainingBuffer;
    readonly Action<EvolutionSetup<T>, IRng, GenePool<T>, int> mutator;

    Population(int size) {
        trainingBuffer = new PopulationEntry<T>[size];
        if (typeof(ICrossChromosome<T>).IsAssignableFrom(typeof(T))) {
            mutator = Cross;
        }
        else if (typeof(IMutatingChromosome<T>).IsAssignableFrom(typeof(T))) {
            mutator = Mutate;
        }
        else throw new NotImplementedException();
    }
    
    /// <summary>
    /// creates a new <see cref="Population{T}"/>
    /// </summary>
    /// <param name="size">size of population</param>
    /// <param name="generator">used to generate new chromosomes</param>
    /// <param name="rng">rng to use to initialize population</param>
    public Population(int size, Func<IRng, T> generator, Rng rng=null)
    : this(size)
    {
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
                                   Chromosome = generator(rng),
                                   AncestryId = Guid.NewGuid()
                               };
            Entries[i].Chromosome.Randomize(crossSetup);
        }
    }

    /// <summary>
    /// creates a new <see cref="Population{T}"/>
    /// </summary>
    /// <param name="population">entries of population</param>
    /// <param name="generator">generator for new chromosome instances</param>
    public Population(PopulationEntry<T>[] population, Func<IRng, T> generator)
    : this(population.Length)
    {
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
        HashSet<int> structureHashes = [];

        int offset = 0;
        foreach (PopulationEntry<T> entry in Entries) {
            if (entry.Fitness < 0.0f)
                continue;
            
            int structureHash = entry.Chromosome.StructureHash();
            if (!structureHashes.Add(structureHash))
                continue;

            trainingBuffer[offset++] = entry;
            
            if (offset >= setup.Elitism)
                break;
        }

        float modifiedMax = Entries.Max(e => (e.Fitness + 1.0f) / e.Chromosome.FitnessModifier);

        GenePool<T> genePool = new(Generator);
        foreach (PopulationEntry<T> entry in Entries) {
            if (entry.Fitness < 0.0)
                continue;

            float value = (modifiedMax - (entry.Fitness + 1.0f) / entry.Chromosome.FitnessModifier) / modifiedMax;
            entry.Fitness = value * value;
            genePool.Add(entry);
        }

        mutator(setup, rng, genePool, offset);
        Parallel.ForEach(trainingBuffer,
                         new() { MaxDegreeOfParallelism = setup.Threads },
                         entry => entry.Fitness = setup.Evaluator.EvaluateFitness(entry.Chromosome, rng, false));
        Array.Sort(trainingBuffer, (lhs, rhs) => GetOrderNumber(lhs).CompareTo(GetOrderNumber(rhs)));
        (Entries, trainingBuffer) = (trainingBuffer, Entries);
    }

    void Cross(EvolutionSetup<T> setup, IRng rng, GenePool<T> genePool, int offset) {
        for (int i = offset; i < trainingBuffer.Length; ++i) {
            PopulationEntry<T> firstChromosome = genePool.Next(rng);
            PopulationEntry<T> secondChromosome = genePool.Next(rng);

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
            else if (i >= Entries.Length - setup.Elitism * 2)
                mutationChance = 0.15f + (float)(Entries.Length - i) / (setup.Elitism * 2);

            float mutationScale = (float)i / Entries.Length;
            mutationRate *= mutationScale;
            mutationRange *= mutationScale;

            T chromosome = ((ICrossChromosome<T>)firstChromosome.Chromosome).Cross(secondChromosome.Chromosome, new() {
                MutateChance = mutationChance,
                MutateRate = mutationRate,
                MutateRange = mutationRange,
                Rng = rng
            });

            trainingBuffer[i] = new() {
                Chromosome = chromosome,
                Fitness = setup.Evaluator.EvaluateFitness(chromosome, rng, false),
                AncestryId = firstChromosome.AncestryId
            };
        }
    }

    void Mutate(EvolutionSetup<T> setup, IRng rng, GenePool<T> genePool, int offset) {
        void Mutator(int i) {
            if (Generator != null && i > trainingBuffer.Length - setup.Elitism) {
                Mutate(trainingBuffer, rng, new() {
                    Chromosome = Generator(rng),
                    AncestryId = Guid.NewGuid() 
                }, setup, i);
            }
            else
                Mutate(trainingBuffer, rng, genePool.Next(rng), setup, i);
        }

        if (setup.Threads > 1) {
            Parallel.For(offset, Entries.Length, new() {
                MaxDegreeOfParallelism = setup.Threads
            }, Mutator);
        }
        else {
            for (int i = offset; i < trainingBuffer.Length; ++i)
                Mutator(i);
        }
    }
    
    void Mutate(PopulationEntry<T>[] next, IRng rng, PopulationEntry<T> current, EvolutionSetup<T> setup, int i) {
        float mutationRange = setup.Mutation.Range;

        float mutationScale = (float)i / Entries.Length;
        mutationRange *= mutationScale;

        T chromosome = current.Chromosome;
        if (setup.Rivalism > 1) {
            List<PopulationEntry<T>> candidates = [];
            
            for (int l = 0; l < setup.Rivalism; ++l) {
                int runs = 1 + rng.NextInt(setup.Mutation.Runs);
                for (int k = 0; k < runs; ++k)
                    chromosome = ((IMutatingChromosome<T>)chromosome).Mutate(rng, mutationRange);
                candidates.Add(new() {
                    Chromosome = chromosome,
                    Fitness = setup.Evaluator.EvaluateFitness(chromosome, rng, false),
                    AncestryId = current.AncestryId
                });
            }
            
            candidates.Sort((lhs, rhs) => GetOrderNumber(lhs).CompareTo(GetOrderNumber(rhs)));
            next[i] = candidates.First();
        }
        else {
            int runs = 1 + rng.NextInt(setup.Mutation.Runs);
            for (int k = 0; k < runs; ++k)
                chromosome = ((IMutatingChromosome<T>)chromosome).Mutate(rng, mutationRange);

            next[i] = new() {
                Chromosome = chromosome,
                AncestryId = current.AncestryId
                //Fitness = setup.Evaluator.EvaluateFitness(chromosome, rng, false)
            };
        }
    }

    float GetOrderNumber(PopulationEntry<T> entry) {
        if (entry.Fitness < 0.0)
            return float.MaxValue;
        return entry.Fitness;
    }
    
    /// <summary>
    /// trains the population
    /// </summary>
    /// <param name="setup">configuration values used for evolution</param>
    /// <returns>best training result</returns>
    public PopulationEntry<T> Train(EvolutionSetup<T> setup) {
        IRng rng = setup.Threads > 1 ? new LockedRng() : new Rng();
        
        // compute fitness for all population entries
        if (setup.Threads > 1)
            Parallel.ForEach(Entries, new() { MaxDegreeOfParallelism = setup.Threads }, entry => entry.Fitness = setup.Evaluator.EvaluateFitness(entry.Chromosome, rng, true));
        else 
            foreach (PopulationEntry<T> entry in Entries)
                entry.Fitness = setup.Evaluator.EvaluateFitness(entry.Chromosome, rng, true);
        
        int bestStructure = Entries[0].Chromosome.StructureHash();
        int bestRun = 0;

        for (int i = 0; i < setup.Runs; i++) {
            Evolve(rng, setup);
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

        Entries[0].Fitness = setup.Evaluator.EvaluateFitness(Entries[0].Chromosome, rng, true);
        return Entries[0];
    }
}