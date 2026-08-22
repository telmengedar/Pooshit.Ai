using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

class FakeChromosome : IChromosome<FakeChromosome> {
    public FakeChromosome(NeuronConfig[] neurons) => Neurons = neurons;

    public void Randomize(CrossSetup setup = null) { }


    public int StructureHash() => 0;


    public float FitnessModifier => 0.0f;


    public FakeChromosome Optimize(Func<FakeChromosome, bool> test) => this;


    public NeuronConfig[] Neurons { get; }
}
