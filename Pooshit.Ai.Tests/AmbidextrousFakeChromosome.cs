using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Neurons;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// a chromosome implementing BOTH <see cref="IMutatingChromosome{T}"/> and <see cref="ICrossChromosome{T}"/>.
/// Exists for exactly one test: that <see cref="Population{T}"/>'s constructor binds the cross strategy
/// and never mutates when a chromosome offers both - a silent, permanent, constructor-time decision today
/// (design #9072 §5.2). Records calls to both members into logs shared across a population so a test can
/// assert one log is non-empty and the other is empty.
/// </summary>
class AmbidextrousFakeChromosome : IChromosome<AmbidextrousFakeChromosome>, IMutatingChromosome<AmbidextrousFakeChromosome>, ICrossChromosome<AmbidextrousFakeChromosome>, ILabelledChromosome {
    readonly List<string> mutateLog;
    readonly List<string> crossLog;
    readonly int structureHash;

    public AmbidextrousFakeChromosome(string label, List<string> mutateLog, List<string> crossLog, int structureHash = 0, float fitnessModifier = 1.0f) {
        Label = label;
        this.mutateLog = mutateLog;
        this.crossLog = crossLog;
        this.structureHash = structureHash;
        FitnessModifier = fitnessModifier;
    }

    public string Label { get; }

    public void Randomize(CrossSetup setup = null) { }


    public int StructureHash() => structureHash;


    public float FitnessModifier { get; }


    public AmbidextrousFakeChromosome Optimize(Func<AmbidextrousFakeChromosome, bool> test) => this;


    public NeuronConfig[] Neurons { get; } = [];


    public AmbidextrousFakeChromosome Mutate(IRng rng, float mutationRange) {
        mutateLog.Add(Label);
        return new($"{Label}'", mutateLog, crossLog, structureHash, FitnessModifier);
    }


    public AmbidextrousFakeChromosome Cross(AmbidextrousFakeChromosome other, CrossSetup setup) {
        crossLog.Add($"{Label}x{other.Label}");
        return new($"{Label}x{other.Label}", mutateLog, crossLog, structureHash, FitnessModifier);
    }
}
