using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class NonFiniteFitnessTests {

    static PopulationEntry<MutatingFakeChromosome> Entry(string label, List<MutateCall> log, float fitness, int structureHash) => new() {
        Chromosome = new(label, log, structureHash, 1.0f),
        Fitness = fitness,
        AncestryId = Guid.NewGuid()
    };


    [Test, Parallelizable]
    [Description("Non-finite fitness must sort after every finite value, never before it (DiVoid #9037).")]
    public void Evolve_NonFiniteRescoredFitness_SortsLastRegardlessOfFiniteMagnitude() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome> e2 = Entry("e2", log, 2.0f, 2);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        int freshCounter = 0;
        Population<MutatingFakeChromosome> population = new(entries, r => new($"fresh{freshCounter++}", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 5.0f,
            ["e1"] = 1.0f,
            ["e2"] = 2.0f,
            ["e0'1"] = float.NaN,
            ["e0'2"] = float.PositiveInfinity,
            ["fresh0'2"] = float.PositiveInfinity
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.0f, 0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries, Has.Length.EqualTo(3));
        Assert.That(population.Entries[0].Chromosome.Label, Is.EqualTo("e0"), "5.0 is the only finite fitness produced this generation");
        Assert.That(population.Entries[0].Fitness, Is.EqualTo(5.0f));
        Assert.That(float.IsFinite(population.Entries[1].Fitness), Is.False, "rank 1 must be one of the non-finite descendants");
        Assert.That(float.IsFinite(population.Entries[2].Fitness), Is.False, "rank 2 must be the other non-finite descendant");
    }


    [Test, Parallelizable]
    [Description("R1 sibling: a finite value in place of NaN must win rank 0 (DiVoid #9037).")]
    public void Evolve_FiniteRescoredFitnessReplacingNaN_WinsRankZero() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 0.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome> e2 = Entry("e2", log, 2.0f, 2);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        int freshCounter = 0;
        Population<MutatingFakeChromosome> population = new(entries, r => new($"fresh{freshCounter++}", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 5.0f,
            ["e1"] = 1.0f,
            ["e2"] = 2.0f,
            ["e0'1"] = 0.001f,
            ["e0'2"] = 2.5f,
            ["fresh0'2"] = 2.5f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.0f, 0.0f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries[0].Chromosome.Label, Is.EqualTo("e0'1"), "0.001 is now the lowest finite fitness, and must win rank 0 now that it is no longer NaN");
    }


    [Test, Parallelizable]
    [Description("NaN, +Infinity and -Infinity must all be excluded from elitism and the gene pool (DiVoid #9037).")]
    public void Evolve_AnyNonFiniteFitnessEntry_ExcludedFromElitismAndGenePool() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> nan = Entry("nan", log, float.NaN, 0);
        PopulationEntry<MutatingFakeChromosome> posInf = Entry("posInf", log, float.PositiveInfinity, 1);
        PopulationEntry<MutatingFakeChromosome> negInf = Entry("negInf", log, float.NegativeInfinity, 2);
        PopulationEntry<MutatingFakeChromosome> valid = Entry("valid", log, 1.0f, 3);
        PopulationEntry<MutatingFakeChromosome>[] entries = [nan, posInf, negInf, valid];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["nan"] = float.NaN,
            ["posInf"] = float.PositiveInfinity,
            ["negInf"] = float.NegativeInfinity,
            ["valid"] = 1.0f,
            ["valid'1"] = 10.0f,
            ["valid'2"] = 11.0f,
            ["valid'3"] = 12.0f,
            ["fresh'3"] = 12.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0, 0),
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries, Has.No.Member(nan), "a NaN-fitness entry must not be carried forward as elite");
        Assert.That(population.Entries, Has.No.Member(posInf), "a +Infinity-fitness entry must not be carried forward as elite");
        Assert.That(population.Entries, Has.No.Member(negInf), "a -Infinity-fitness entry must not be carried forward as elite");
        Assert.That(population.Entries, Has.Member(valid), "the sole elite must be the first valid entry");
        Assert.That(population.Entries.Any(entry => entry.Chromosome.Label.Contains("nan") || entry.Chromosome.Label.Contains("Inf")), Is.False,
                    "no gene-pool draw may have descended from a non-finite-fitness entry");
    }


    [Test, Parallelizable]
    [Description("R6 oracle: a NaN entry must not change the breeding weight of other, finite entries (DiVoid #9037).")]
    public void Evolve_NaNEntryAddedToPopulation_DoesNotChangeBreedingWeightsOfFiniteEntries() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = new() { Chromosome = new("e0", log, 0, fitnessModifier: 2.0f), Fitness = 1.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e1 = new() { Chromosome = new("e1", log, 1, fitnessModifier: 1.0f), Fitness = 3.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> poison = new() { Chromosome = new("poison", log, 2, fitnessModifier: 1.0f), Fitness = float.NaN, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, poison];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 1.0f,
            ["e1"] = 3.0f,
            ["poison"] = float.NaN,
            ["e0'1"] = 10.0f,
            ["e0'2"] = 11.0f,
            ["fresh'2"] = 11.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.9f, 0.9f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries, Has.Length.EqualTo(3));
        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label), Does.Contain("e0"));
        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label), Does.Contain("e0'1"),
                    "cumulative selector 0.9 * 0.5625 = 0.50625 falls in e0's [0, 0.5625) bracket, exactly as it would if poison were absent");
    }


    [Test, Parallelizable]
    [Description("R1 sibling of the NaN breeding-weight invariance test: the NaN entry is replaced by a finite entry sitting exactly at modifiedMax, so it carries zero breeding weight (DiVoid #9037).")]
    public void Evolve_NaNEntryReplacedByZeroWeightFiniteEntry_ProducesIdenticalBreedingOutcome() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = new() { Chromosome = new("e0", log, 0, fitnessModifier: 2.0f), Fitness = 1.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e1 = new() { Chromosome = new("e1", log, 1, fitnessModifier: 1.0f), Fitness = 3.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e2 = new() { Chromosome = new("e2", log, 2, fitnessModifier: 1.0f), Fitness = 3.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, e2];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 1.0f,
            ["e1"] = 3.0f,
            ["e2"] = 3.0f,
            ["e0'1"] = 10.0f,
            ["fresh'1"] = 10.0f,
            ["e0'2"] = 11.0f,
            ["fresh'2"] = 11.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.9f, 0.9f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label), Is.EquivalentTo(new[] { "e0", "e0'1", "fresh'2" }),
                    "modifiedMax = 4.0 and weight(e0) = 0.5625 are unchanged by the replacement, so the gene-pool slot still draws e0 - the identical selection outcome the poisoned test relies on");
    }


    [Test, Parallelizable]
    [Description("The +Infinity analogue of the NaN breeding-weight invariance test (DiVoid #9037).")]
    public void Evolve_PositiveInfinityEntryAddedToPopulation_DoesNotChangeBreedingWeightsOfFiniteEntries() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = new() { Chromosome = new("e0", log, 0, fitnessModifier: 2.0f), Fitness = 1.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e1 = new() { Chromosome = new("e1", log, 1, fitnessModifier: 1.0f), Fitness = 3.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> poison = new() { Chromosome = new("poison", log, 2, fitnessModifier: 1.0f), Fitness = float.PositiveInfinity, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1, poison];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 1.0f,
            ["e1"] = 3.0f,
            ["poison"] = float.PositiveInfinity,
            ["e0'1"] = 10.0f,
            ["e0'2"] = 11.0f,
            ["fresh'2"] = 11.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0) { FloatValues = [0.9f, 0.9f] },
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(population.Entries, Has.Length.EqualTo(3));
        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label), Does.Contain("e0"));
        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label), Does.Contain("e0'1"),
                    "cumulative selector 0.9 * 0.5625 = 0.50625 falls in e0's [0, 0.5625) bracket, exactly as it would if poison were absent - modifiedMax = 4.0 must be computed only over e0 and e1");
    }


    [Test, Parallelizable]
    [Description("A population where every entry is non-finite must not throw or return a disqualified winner (DiVoid #9037).")]
    public void Evolve_EveryEntryNonFinite_DoesNotThrowAndDoesNotReturnANonFiniteWinner() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> nan = Entry("nan", log, float.NaN, 0);
        PopulationEntry<MutatingFakeChromosome> posInf = Entry("posInf", log, float.PositiveInfinity, 1);
        PopulationEntry<MutatingFakeChromosome>[] entries = [nan, posInf];

        int freshCounter = 0;
        Population<MutatingFakeChromosome> population = new(entries, r => new($"fresh{freshCounter++}", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["nan"] = float.NaN,
            ["posInf"] = float.PositiveInfinity,
            ["fresh0'1"] = 4.0f,
            ["fresh1'2"] = 3.0f
        };
        StubFitnessEvaluator<MutatingFakeChromosome> evaluator = new(fitnessByLabel);
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new SequenceRng(0, 0),
            Threads = 1,
            Runs = 1,
            Elitism = 1,
            TargetFitness = -1.0f
        };

        Assert.DoesNotThrow(() => population.Train(setup));

        Assert.That(population.Entries, Has.No.Member(nan), "the original NaN entry must not survive as a disqualified winner");
        Assert.That(population.Entries, Has.No.Member(posInf), "the original +Infinity entry must not survive as a disqualified winner");
        Assert.That(population.Entries[0].Chromosome.Label, Is.EqualTo("fresh1'2"), "the winner must be the honestly re-scored, freshly generated chromosome with the lower fitness");
        Assert.That(population.Entries[0].Fitness, Is.EqualTo(3.0f), "the winner's fitness must be the honestly re-scored value, not a leftover non-finite value");
    }


    [Test, Parallelizable]
    [Description("OnNonFiniteFitness must report the poisoned entry's exclusion and never let it reappear (DiVoid #9037).")]
    public void Train_OneEntryStructurallyNonFinite_OnNonFiniteFitnessCountDropsToZeroAndStaysThere() {
        List<MutateCall> log = [];
        const int poisonedHash = 0;
        const int healthyHash = 1;
        PopulationEntry<MutatingFakeChromosome> poisoned = Entry("poisoned", log, float.NaN, poisonedHash);
        PopulationEntry<MutatingFakeChromosome> healthy = Entry("healthy", log, 2.0f, healthyHash);
        PopulationEntry<MutatingFakeChromosome>[] entries = [poisoned, healthy];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, healthyHash, 1.0f));

        PhaseAwareFitnessEvaluator<MutatingFakeChromosome> evaluator = new((structureHash, _) => structureHash == poisonedHash ? float.NaN : 2.0f);

        List<(int Generation, int Count)> observations = [];
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = evaluator,
            Rng = new Rng(20260825),
            Threads = 1,
            Runs = 3,
            Elitism = 1,
            TargetFitness = -1.0f,
            OnNonFiniteFitness = (generation, count) => observations.Add((generation, count))
        };

        population.Train(setup);

        Assert.That(observations, Has.Count.EqualTo(3));
        Assert.That(observations[0], Is.EqualTo((0, 1)), "generation 0 enters with exactly the one originally-poisoned entry present");
        Assert.That(observations.Skip(1), Is.All.Matches<(int Generation, int Count)>(o => o.Count == 0),
                    "once excluded from elitism and the gene pool the poisoned entry cannot reappear as a descendant in any later generation");
        Assert.That(population.Entries[0].Chromosome.Label, Is.Not.EqualTo("poisoned"), "the rescue property this whole test exists to demonstrate: the winner must not be the poisoned entry");
    }


    [Test, Parallelizable]
    [Description("R1 sibling: an all-finite run must report zero every generation (DiVoid #9037).")]
    public void Train_AllEntriesFiniteAcrossGenerations_OnNonFiniteFitnessReportsZeroEveryGeneration() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = Entry("e0", log, 1.0f, 0);
        PopulationEntry<MutatingFakeChromosome> e1 = Entry("e1", log, 1.0f, 1);
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        List<int> observedCounts = [];
        EvolutionSetup<MutatingFakeChromosome> setup = new() {
            Evaluator = new PhaseAwareFitnessEvaluator<MutatingFakeChromosome>((_, _) => 1.0f),
            Rng = new Rng(20260825),
            Threads = 1,
            Runs = 10,
            Elitism = 1,
            TargetFitness = -1.0f,
            OnNonFiniteFitness = (generation, count) => observedCounts.Add(count)
        };

        population.Train(setup);

        Assert.That(observedCounts, Has.Count.EqualTo(10));
        Assert.That(observedCounts, Is.All.EqualTo(0));
    }
}
