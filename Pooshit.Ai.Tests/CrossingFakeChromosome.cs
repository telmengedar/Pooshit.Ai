using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// one entry in <see cref="CrossingFakeChromosome"/>'s reproduction log
/// </summary>
/// <param name="FirstLabel">label of the receiver <see cref="CrossingFakeChromosome.Cross"/> was called on</param>
/// <param name="SecondLabel">label of the chromosome it was crossed with</param>
/// <param name="MutateChance">the <see cref="CrossSetup.MutateChance"/> the call received</param>
/// <param name="MutateRate">the <see cref="CrossSetup.MutateRate"/> the call received</param>
/// <param name="MutateRange">the <see cref="CrossSetup.MutateRange"/> the call received</param>
/// <param name="Ordinal">position of this call in the shared log</param>
record CrossCall(string FirstLabel, string SecondLabel, float MutateChance, float MutateRate, float MutateRange, int Ordinal);

/// <summary>
/// a chromosome implementing only <see cref="ICrossChromosome{T}"/>, so <see cref="Population{T}"/>
/// binds the cross reproduction strategy. Records every <see cref="Cross"/> call, including the
/// exact <see cref="CrossSetup"/> mutation parameters it received, into a log shared by every
/// instance descended from the same population (design #9072 §5.2 / §8.2)
/// </summary>
class CrossingFakeChromosome : IChromosome<CrossingFakeChromosome>, ICrossChromosome<CrossingFakeChromosome>, ILabelledChromosome {
    readonly List<CrossCall> log;
    readonly int structureHash;

    public CrossingFakeChromosome(string label, List<CrossCall> log, int structureHash = 0, float fitnessModifier = 1.0f) {
        Label = label;
        this.log = log;
        this.structureHash = structureHash;
        FitnessModifier = fitnessModifier;
    }

    public string Label { get; }

    public void Randomize(CrossSetup setup = null) { }


    public int StructureHash() => structureHash;


    public float FitnessModifier { get; }


    public CrossingFakeChromosome Optimize(Func<CrossingFakeChromosome, bool> test) => this;


    public NeuronConfig[] Neurons { get; } = [];


    /// <summary>
    /// records the call, including the exact mutation parameters received, and returns a new,
    /// distinctly labelled instance. Never mutates either parent.
    /// </summary>
    public CrossingFakeChromosome Cross(CrossingFakeChromosome other, CrossSetup setup) {
        log.Add(new(Label, other.Label, setup.MutateChance, setup.MutateRate, setup.MutateRange, log.Count));
        return new($"{Label}x{other.Label}", log, structureHash, FitnessModifier);
    }
}
