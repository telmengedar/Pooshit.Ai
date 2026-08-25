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
    [Description("Re-scored fitness values include NaN and +Infinity so the disqualify guard, not fixture order, explains why they sort last; e0 (the only finite re-scored entry) taking rank 0 with both non-finite descendants trailing proves neither non-finite kind can win the sort (DiVoid #9037).")]
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
            ["e0'2"] = float.PositiveInfinity
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
        Assert.That(new[] { population.Entries[1].Chromosome.Label, population.Entries[2].Chromosome.Label },
                    Is.EquivalentTo(new[] { "e0'1", "e0'2" }), "NaN and +Infinity must both sort after every finite value, in either order");
    }


    [Test, Parallelizable]
    [Description("R1 sibling of Evolve_NonFiniteRescoredFitness_SortsLastRegardlessOfFiniteMagnitude: the only change is e0'1's rescored fitness moving from NaN to a small finite value. That value must then win rank 0 - proving the guard is keyed on non-finiteness specifically, not on some other property of this fixture.")]
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
            ["e0'2"] = 2.5f
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
    [Description("NaN, +Infinity and -Infinity fitness entries must ALL be excluded from elitism and the gene pool. NaN and +Infinity are the two non-finite routes named in DiVoid #9037; -Infinity is the third IEEE-754 non-finite value the float.IsFinite guard also covers. Elitism = 1 leaves three mutate slots drawing from a gene pool that must contain only 'valid' - if any non-finite entry leaked into elitism or the pool, its label or a descendant's would appear in the final population.")]
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
            ["valid'3"] = 12.0f
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
    [Description("Independent R6 oracle + R1 invariance sibling: hand-computed breeding weights for two finite entries (modifiedMax = (3+1)/1 = 4.0, weight(e0) = ((4-1)/4)^2 = 0.5625, weight(e1) = ((4-4)/4)^2 = 0) predict which parent a scripted GenePool draw selects. A NaN entry added to the SAME population must not change modifiedMax or the draw - proving it is excluded from the modifiedMax computation, not merely from its own breeding weight (design #9072's precision note on modifiedMax; DiVoid #9037). Under the pre-fix code this fixture throws NotSupportedException, because Enumerable.Max propagates the NaN into modifiedMax regardless of position, which poisons every FitnessSelector to NaN and forces GenePool.Next's FirstOrDefault fallback to consume an unscripted extra rng draw.")]
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
            ["e0'2"] = 11.0f
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

        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label),
                    Is.EquivalentTo(new[] { "e0", "e0'1", "e0'2" }),
                    "cumulative selector 0.9 * 0.5625 = 0.50625 falls in e0's [0, 0.5625) bracket for both mutate slots, exactly as it would if poison were absent");
    }


    [Test, Parallelizable]
    [Description("R1 sibling of Evolve_NaNEntryAddedToPopulation_DoesNotChangeBreedingWeightsOfFiniteEntries: identical fixture with the NaN entry removed entirely. Both tests must reach the SAME breeding outcome, proving the NaN entry's presence is provably inert rather than merely 'probably harmless'.")]
    public void Evolve_SameEntriesWithoutNaN_ProducesIdenticalBreedingOutcome() {
        List<MutateCall> log = [];
        PopulationEntry<MutatingFakeChromosome> e0 = new() { Chromosome = new("e0", log, 0, fitnessModifier: 2.0f), Fitness = 1.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome> e1 = new() { Chromosome = new("e1", log, 1, fitnessModifier: 1.0f), Fitness = 3.0f, AncestryId = Guid.NewGuid() };
        PopulationEntry<MutatingFakeChromosome>[] entries = [e0, e1];

        Population<MutatingFakeChromosome> population = new(entries, r => new("fresh", log, fitnessModifier: 1.0f));

        Dictionary<string, float> fitnessByLabel = new() {
            ["e0"] = 1.0f,
            ["e1"] = 3.0f,
            ["e0'1"] = 10.0f,
            ["e0'2"] = 11.0f
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

        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label), Is.EquivalentTo(new[] { "e0", "e0'1" }),
                    "with poison absent there is one fewer mutate slot (population size 2, not 3), but the SAME modifiedMax = 4.0 and weight(e0) = 0.5625 must still draw e0 as parent - the identical selection outcome the poisoned test relies on");
    }


    [Test, Parallelizable]
    [Description("Same shape and same hand-computed oracle as Evolve_NaNEntryAddedToPopulation_DoesNotChangeBreedingWeightsOfFiniteEntries, but the poison is +Infinity rather than NaN, and there are two valid entries so GenePool.Next exercises the cumulative-selector branch rather than its Count==1 shortcut. This distinction is load-bearing: on this runtime Enumerable.Max<float> silently ignores NaN but DOES propagate +Infinity, so a NaN poison alone never actually exercises the modifiedMax filter this test targets. Under the pre-fix code, unfiltered modifiedMax becomes +Infinity, (Infinity - finite)/Infinity is NaN for BOTH e0 and e1, and GenePool.Next's FirstOrDefault comparison against a NaN selector never succeeds for either entry - forcing its fallback branch to consume rng draws this scripted rng did not provision, so the mutant throws NotSupportedException instead of drawing e0 as predicted.")]
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
            ["e0'2"] = 11.0f
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

        Assert.That(population.Entries.Select(entry => entry.Chromosome.Label),
                    Is.EquivalentTo(new[] { "e0", "e0'1", "e0'2" }),
                    "cumulative selector 0.9 * 0.5625 = 0.50625 falls in e0's [0, 0.5625) bracket for both mutate slots, exactly as it would if poison were absent - modifiedMax = 4.0 must be computed only over e0 and e1");
    }


    [Test, Parallelizable]
    [Description("Every original entry is non-finite (NaN and +Infinity), so elitism finds zero valid entries and the breeding-weight loop's modifiedMax computation must survive an empty finite-entries sequence without throwing InvalidOperationException from LINQ Max on an empty source. Both mutate slots then fall back to GenePool's empty-pool path, which generates and mutates a brand-new chromosome per slot - so Entries[0] must be a genuinely fresh, finitely-scored chromosome, never one of the original non-finite entries smuggled through as a disqualified winner (the all-invalid analogue named in DiVoid #9037's coverage list, mirroring #9054 item 6's all-negative case).")]
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
    [Description("The one entry whose StructureHash scores NaN is excluded from elitism and the gene pool the very first generation it appears, so it can never again be selected as a reproduction parent - the count observed by OnNonFiniteFitness must drop from 1 to 0 and STAY at 0, demonstrating the exact mechanism #9060 identifies as the 'never heals itself' absorbing state is now escapable.")]
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
    }


    [Test, Parallelizable]
    [Description("R1 sibling of Train_OneEntryStructurallyNonFinite_OnNonFiniteFitnessCountDropsToZeroAndStaysThere: every entry stays finite for the whole run, so every observed count must be zero - proving the hook actually counts non-finite entries rather than firing a constant or a stale value.")]
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
