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
    [Description("Every chromosome shares one StructureHash across 70 generations so only Train's escalation branch (never the reset branch) can fire, pinning the documented schedule Math.Min(50, 1 + ((i - bestRun) >> 6) * 5).")]
    public void Train_LeaderStructureHashUnchangedFor64Generations_MutationRunsEscalatesOnDocumentedSchedule() {
        List<MutateCall> log = [];
        const int constantStructureHash = 42;
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, constantStructureHash);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, constantStructureHash);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, constantStructureHash, 1.0f));

        List<int> mutationRunsAfterEachGeneration = [];
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = new ConstantFitnessEvaluator<MutatingFakeChromosome>(1.0f),
            Rng = new Rng(20260824),
            Threads = 1,
            Runs = 70,
            Elitism = 1,
            TargetFitness = -1.0f
        };
        setup.AfterRun = (generation, fitness) => mutationRunsAfterEachGeneration.Add(setup.Mutation.Runs);

        population.Train(setup);

        Assert.That(mutationRunsAfterEachGeneration, Has.Count.EqualTo(70));
        Assert.That(mutationRunsAfterEachGeneration[63], Is.EqualTo(1), "generation 63 (i - bestRun = 63) is still below the 64-generation escalation threshold");
        Assert.That(mutationRunsAfterEachGeneration[64], Is.EqualTo(6), "generation 64 (i - bestRun = 64) crosses the threshold: 1 + (64 >> 6) * 5 = 6");
    }


    [Test, Parallelizable]
    [Description("Escalates on StructureHash 1 for 69 generations (reaching Runs = 6), then flips scoring via AfterRun so a fresh-blood entry (StructureHash 99) becomes the new leader at generation 70, forcing a reset to 1 and proving escalation resumes counting from the new bestRun (generation 134, not the original 64) rather than a stale one.")]
    public void Train_LeaderStructureHashChangesAfterEscalating_MutationRunsResetsThenReescalatesFromTheNewBestRun() {
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

        List<int> mutationRunsAfterEachGeneration = [];
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new Rng(20260824),
            Threads = 1,
            Runs = 140,
            Elitism = 2,
            TargetFitness = -1.0f
        };
        setup.AfterRun = (generation, fitness) => {
            mutationRunsAfterEachGeneration.Add(setup.Mutation.Runs);
            if (generation == 69)
                evaluator.Phase = 1;
        };

        population.Train(setup);

        Assert.That(mutationRunsAfterEachGeneration[69], Is.EqualTo(6), "phase 0 must have escalated before the structure change, or the reset that follows proves nothing");
        Assert.That(mutationRunsAfterEachGeneration[70], Is.EqualTo(1), "the fresh-blood entry outscoring the elite changes the leader's StructureHash, which must reset Mutation.Runs");
        Assert.That(mutationRunsAfterEachGeneration[133], Is.EqualTo(1), "generation 133 (i - bestRun = 63) is still below the re-anchored escalation threshold");
        Assert.That(mutationRunsAfterEachGeneration[134], Is.EqualTo(6), "generation 134 (i - bestRun = 64) crosses the threshold measured from the NEW bestRun (70), not the original (0)");
    }
}
