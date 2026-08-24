using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

class FakeChromosome : IChromosome<FakeChromosome>, ILabelledChromosome {
    readonly int structureHash;

    public FakeChromosome(NeuronConfig[] neurons, string label = "", int structureHash = 0, float fitnessModifier = 1.0f) {
        Neurons = neurons;
        Label = label;
        this.structureHash = structureHash;
        FitnessModifier = fitnessModifier;
    }

    public string Label { get; }

    public void Randomize(CrossSetup setup = null) { }


    public int StructureHash() => structureHash;


    public float FitnessModifier { get; }


    public FakeChromosome Optimize(Func<FakeChromosome, bool> test) => this;


    public NeuronConfig[] Neurons { get; }
}
