using Pooshit.Ai.Net.Operations;

namespace Pooshit.Ai.Genetics.Mutation;

public class ActivationFuncOptions : MutationOptions<ActivationFunc> {
    
    /// <inheritdoc />
    public ActivationFuncOptions(params MutationEntry<ActivationFunc>[] entries)
        : base(entries) {
        if (entries.Length == 0)
            GenerateDefaults();
    }

    protected sealed override void GenerateDefaults() {
        SetEntries(new MutationEntry<ActivationFunc>(ActivationFunc.None, 1.0),
                   new MutationEntry<ActivationFunc>(ActivationFunc.BinaryStep, 0.1),
                   new MutationEntry<ActivationFunc>(ActivationFunc.Sigmoid, 0.15),
                   new MutationEntry<ActivationFunc>(ActivationFunc.Sin, 0.05),
                   new MutationEntry<ActivationFunc>(ActivationFunc.ReLU, 0.01),
                   new MutationEntry<ActivationFunc>(ActivationFunc.LeakyReLU, 0.01),
                   new MutationEntry<ActivationFunc>(ActivationFunc.Tanh, 0.01),
                   new MutationEntry<ActivationFunc>(ActivationFunc.Reciprocal, 0.1),
                   new MutationEntry<ActivationFunc>(ActivationFunc.Swish, 0.01));
    }
}