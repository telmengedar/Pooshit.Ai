using System.Collections.Concurrent;
using Pooshit.Ai.Extern;

namespace Pooshit.Ai.Genetics;

/// <summary>
/// pool of candidates for evolution
/// </summary>
/// <typeparam name="T">type of contained chromosome</typeparam>
public class GenePool<T> where T : IChromosome<T> {
	float fitnessSum;
	readonly List<PopulationEntry<T>> population = [];
	readonly Dictionary<Guid, int> originCount = new();
	readonly Func<IRng, T> generator;

	object poolLock = new();
	/// <summary>
	/// creates a new <see cref="GenePool{T}"/>
	/// </summary>
	/// <param name="generator">generator for new chromosomes</param>
	public GenePool(Func<IRng, T> generator) => this.generator = generator;

	void RecomputeFitnessSelector() {
		fitnessSum = 0.0f;
		foreach (PopulationEntry<T> entry in population) {
			fitnessSum+=entry.Fitness;
			entry.FitnessSelector = fitnessSum;
		}
	}
	
	/// <summary>
	/// adds an entry to the gene pool
	/// </summary>
	/// <param name="entry">entry to add</param>
	public void Add(PopulationEntry<T> entry) {
		fitnessSum += entry.Fitness;
		entry.FitnessSelector = fitnessSum;
		population.Add(entry);
	}

	/// <summary>
	/// removes all entries with a specific ancestry id
	/// </summary>
	/// <param name="ancestryId">ancestry id to remove from gene pool</param>
	public void Remove(Guid ancestryId) {
		PopulationEntry<T>[] entries = population.Where(p => p.AncestryId == ancestryId).ToArray();
		foreach (PopulationEntry<T> entry in entries)
			population.Remove(entry);
		RecomputeFitnessSelector();
	}

	/// <summary>
	/// selects the next entry from the gene pool
	/// </summary>
	/// <param name="rng">rng to use for selection</param>
	/// <returns>selected entry</returns>
	public PopulationEntry<T> Next(IRng rng) {
		if (population.Count == 0)
			return new() {
				Chromosome = generator(rng),
				AncestryId = Guid.NewGuid()
			};

		PopulationEntry<T> next;
		lock (poolLock) {

			if (population.Count == 1)
				next = population[0];
			else {
				float selectorValue = rng.NextFloat() * fitnessSum;
				next = population.FirstOrDefault(e => e.FitnessSelector >= selectorValue) ?? population[rng.NextInt(population.Count)];
			}


			int count = originCount.GetValueOrDefault(next.AncestryId, 0);
			if (++count >= 5)
				Remove(next.AncestryId);

			originCount[next.AncestryId] = count;
		}

		return next;
	}
}