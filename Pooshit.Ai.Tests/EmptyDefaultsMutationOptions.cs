using Pooshit.Ai.Genetics.Mutation;

namespace NightlyCode.Ai.Tests;

class EmptyDefaultsMutationOptions<T> : MutationOptions<T> {
    public EmptyDefaultsMutationOptions(params MutationEntry<T>[] entries) : base(entries) { }


    protected override void GenerateDefaults() { }
}
