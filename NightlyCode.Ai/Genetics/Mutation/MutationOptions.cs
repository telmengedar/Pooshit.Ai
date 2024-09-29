using NightlyCode.Ai.Extern;

namespace NightlyCode.Ai.Genetics.Mutation;

/// <summary>
/// options for mutation cases
/// </summary>
/// <typeparam name="T">item type to select</typeparam>
public abstract class MutationOptions<T> {
    MutationEntry<T>[] entries;
    double sum;

    /// <summary>
    /// creates new <see cref="MutationOptions{T}"/>
    /// </summary>
    /// <param name="entries">options for mutation</param>
    public MutationOptions(params MutationEntry<T>[] entries) {
        SetEntries(entries);
    }
    
    protected void SetEntries(params T[] entries) {
        this.entries = entries.Select(t => new MutationEntry<T>(t, 1.0)).ToArray();
        sum = entries.Length;
        double value = 0.0;
        foreach (MutationEntry<T> entry in this.entries) {
            value += entry.Weight;
            entry.Weight = value;
        }
    }

    protected void SetEntries(params MutationEntry<T>[] entries) {
        this.entries = entries;
        sum = entries.Sum(e => e.Weight);
        double value = 0.0;
        foreach (MutationEntry<T> entry in entries) {
            value += entry.Weight;
            entry.Weight = value;
        }
    }
    
    /// <summary>
    /// selects an item based on the mutation probability
    /// </summary>
    /// <returns>selected item</returns>
    public T SelectItem(Rng rng) {
        if (entries.Length == 0)
            return default;
        if (entries.Length == 1)
            return entries[0].Item;
        
        double random = rng.NextDouble() * sum;
        foreach (MutationEntry<T> entry in entries)
            if (random <= entry.Weight) 
                return entry.Item;
        
        return default;
    }

    /// <summary>
    /// fills the options with default data
    /// </summary>
    protected abstract void GenerateDefaults();
}