namespace Pooshit.Ai.Genetics.Mutation;

/// <summary>
/// entry in mutation probability list
/// </summary>
/// <typeparam name="T">type of item</typeparam>
public class MutationEntry<T> {
    
    /// <summary>
    /// creates a new <see cref="MutationEntry{T}"/>
    /// </summary>
    /// <param name="item">item to be selected in mutation case</param>
    /// <param name="weight">probability item gets selected</param>
    public MutationEntry(T item, double weight) {
        Item = item;
        Weight = weight;
    }

    /// <summary>
    /// item to be selected in mutation case
    /// </summary>
    public T Item { get; }
    
    /// <summary>
    /// probability item gets selected
    /// </summary>
    public double Weight { get; set; }
}