using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// one entry in <see cref="MutatingFakeChromosome"/>'s reproduction log
/// </summary>
/// <param name="ReceiverLabel">label of the chromosome <see cref="MutatingFakeChromosome.Mutate"/> was called on</param>
/// <param name="Range">mutation range the call received</param>
/// <param name="Ordinal">position of this call in the shared log</param>
record MutateCall(string ReceiverLabel, float Range, int Ordinal);

/// <summary>
/// a chromosome implementing only <see cref="IMutatingChromosome{T}"/>, so <see cref="Population{T}"/>
/// binds the mutate reproduction strategy. Records every <see cref="Mutate"/> call into a log shared
/// by every instance descended from the same population, so a test can see who was drawn, how often,
/// and at what range (design #9072 §5.2 / §8.2)
/// </summary>
class MutatingFakeChromosome : IChromosome<MutatingFakeChromosome>, IMutatingChromosome<MutatingFakeChromosome>, ILabelledChromosome {
    readonly List<MutateCall> log;
    readonly int structureHash;

    public MutatingFakeChromosome(string label, List<MutateCall> log, int structureHash = 0, float fitnessModifier = 1.0f) {
        Label = label;
        this.log = log;
        this.structureHash = structureHash;
        FitnessModifier = fitnessModifier;
    }

    public string Label { get; }

    public void Randomize(CrossSetup setup = null) { }


    public int StructureHash() => structureHash;


    public float FitnessModifier { get; }


    public MutatingFakeChromosome Optimize(Func<MutatingFakeChromosome, bool> test) => this;


    public NeuronConfig[] Neurons { get; } = [];


    /// <summary>
    /// records the call and returns a new, distinctly labelled instance. Never mutates the receiver -
    /// reproduction must be pure and thread-safe (design #9072 §5.2, DiVoid #9029)
    /// </summary>
    public MutatingFakeChromosome Mutate(IRng rng, float mutationRange) {
        log.Add(new(Label, mutationRange, log.Count));
        return new($"{Label}'{log.Count}", log, structureHash, FitnessModifier);
    }
}
