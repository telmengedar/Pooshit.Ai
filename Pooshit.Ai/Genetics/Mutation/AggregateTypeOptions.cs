using Pooshit.Ai.Net.Operations;

namespace Pooshit.Ai.Genetics.Mutation;

/// <inheritdoc />
public class AggregateTypeOptions : MutationOptions<AggregateType> {

    /// <inheritdoc />
    public AggregateTypeOptions(params MutationEntry<AggregateType>[] entries)
        : base(entries) {
        if (entries.Length == 0)
            GenerateDefaults();
    }

    /// <inheritdoc />
    protected sealed override void GenerateDefaults() {
        SetEntries(new MutationEntry<AggregateType>(AggregateType.Sum, 1.0), 
                   new MutationEntry<AggregateType>(AggregateType.Average, 0.6), 
                   new MutationEntry<AggregateType>(AggregateType.Median, 0.2), 
                   new MutationEntry<AggregateType>(AggregateType.Min, 0.05), 
                   new MutationEntry<AggregateType>(AggregateType.Max, 0.05));
    }
}