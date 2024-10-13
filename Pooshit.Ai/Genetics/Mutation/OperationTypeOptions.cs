using Pooshit.Ai.Net.Operations;

namespace Pooshit.Ai.Genetics.Mutation;

/// <inheritdoc />
public class OperationTypeOptions : MutationOptions<OperationType> {
    
    /// <inheritdoc />
    public OperationTypeOptions(params MutationEntry<OperationType>[] entries)
        : base(entries) {
        if(entries.Length==0)
            GenerateDefaults();
    }

    /// <inheritdoc />
    protected sealed override void GenerateDefaults() {
        SetEntries(new MutationEntry<OperationType>(OperationType.Multiply, 1.0),
                   new MutationEntry<OperationType>(OperationType.Add, 1.0),
                   new MutationEntry<OperationType>(OperationType.Div, 0.8),
                   new MutationEntry<OperationType>(OperationType.Sub, 0.8));
    }
}