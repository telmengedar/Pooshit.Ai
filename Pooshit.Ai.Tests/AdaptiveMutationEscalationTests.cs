using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class AdaptiveMutationEscalationTests {

    static PopulationEntry<MutatingFakeChromosome> Entry(string label, List<MutateCall> log, float fitness, int structureHash) => new() {
        Chromosome = new(label, log, structureHash, 1.0f),
        Fitness = fitness,
        AncestryId = Guid.NewGuid()
    };


    [Test, Parallelizable]
    [Description("Every chromosome shares one StructureHash across 70 generations so only Train's escalation branch (never the reset branch) can fire, pinning the documented schedule Math.Min(50, 1 + ((i - bestRun) >> 6) * 5) as the mutation depth bound the mutate strategy requests from the Rng.")]
    public void Train_LeaderStructureHashUnchangedFor64Generations_MutationDepthBoundEscalatesOnDocumentedSchedule() {
        List<MutateCall> log = [];
        const int constantStructureHash = 42;
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, constantStructureHash);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, constantStructureHash);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, constantStructureHash, 1.0f));

        RecordingRng rng = new(new Rng(20260824));
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = new PhaseAwareFitnessEvaluator<MutatingFakeChromosome>((_, _) => 1.0f),
            Rng = rng,
            Threads = 1,
            Runs = 70,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(rng.Bounds, Has.Count.EqualTo(70), "population size 2 at Elitism 1 reproduces exactly one slot per generation, so each generation contributes exactly one mutation depth draw");
        Assert.That(rng.Bounds[64], Is.EqualTo(1), "generation 64 runs on the depth set after generation 63 (i - bestRun = 63), still below the 64-generation escalation threshold");
        Assert.That(rng.Bounds[65], Is.EqualTo(6), "generation 65 runs on the depth set after generation 64 (i - bestRun = 64), which crosses the threshold: 1 + (64 >> 6) * 5 = 6");
        Assert.That(setup.Mutation.Runs, Is.EqualTo(1), "the escalated depth lives in local training state, so the configured default reaches the caller unchanged");
    }


    [Test, Parallelizable]
    [Description("Escalates on StructureHash 1 for 70 generations (reaching a depth bound of 6), then flips scoring via AfterRun so a fresh-blood entry (StructureHash 99) becomes the new leader at generation 70, forcing a reset to 1 and proving escalation resumes counting from the new bestRun (bound 6 at generation 135, not 65) rather than a stale one.")]
    public void Train_LeaderStructureHashChangesAfterEscalating_MutationDepthBoundResetsThenReescalatesFromTheNewBestRun() {
        List<MutateCall> log = [];
        const int baselineStructureHash = 1;
        const int freshBloodStructureHash = 99;
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, baselineStructureHash);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 0.0f, baselineStructureHash);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1];

        Population<MutatingFakeChromosome> population = new(entries, r => new("FRESH", log, freshBloodStructureHash, 1.0f));

        PhaseAwareFitnessEvaluator<MutatingFakeChromosome> evaluator = new((structureHash, phase) => structureHash switch {
            baselineStructureHash => phase == 0 ? 1.0f : 100.0f,
            freshBloodStructureHash => phase == 0 ? 100.0f : 0.0f,
            _ => throw new InvalidOperationException($"unexpected StructureHash {structureHash} in this fixture")
        });

        RecordingRng rng = new(new Rng(20260824));
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = rng,
            Threads = 1,
            Runs = 140,
            Elitism = 1,
            TargetFitness = -1.0f
        };
        setup.AfterRun = (generation, _) => {
            if (generation == 69)
                evaluator.Phase = 1;
        };

        population.Train(setup);

        Assert.That(rng.Bounds, Has.Count.EqualTo(140), "population size 2 at Elitism 1 reproduces exactly one slot per generation, so each generation contributes exactly one mutation depth draw");
        Assert.That(rng.Bounds[70], Is.EqualTo(6), "phase 0 must have escalated before the structure change, or the reset that follows proves nothing");
        Assert.That(rng.Bounds[71], Is.EqualTo(1), "the fresh-blood entry outscoring the elite changes the leader's StructureHash at generation 70, which must reset the depth generation 71 runs on");
        Assert.That(rng.Bounds[134], Is.EqualTo(1), "generation 134 runs on the depth set after generation 133 (i - bestRun = 63), still below the re-anchored escalation threshold");
        Assert.That(rng.Bounds[135], Is.EqualTo(6), "generation 135 runs on the depth set after generation 134 (i - bestRun = 64), measured from the NEW bestRun (70), not the original (0)");
    }
}
