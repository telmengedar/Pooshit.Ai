using Pooshit.Ai.Genetics;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class GenePoolTests {

    static PopulationEntry<FakeChromosome> Entry(float fitness, Guid ancestryId) => new() {
        Chromosome = new([], fitness.ToString(), fitnessModifier: 1.0f),
        Fitness = fitness,
        AncestryId = ancestryId
    };


    [Test, Parallelizable]
    [Description("A single-entry pool returns that entry via the Count==1 shortcut without consuming the rng, so an unscripted SequenceRng safely drives all 5 draws before the lineage retires.")]
    public void Next_LineageDrawnFiveTimes_IsRemovedFromPool() {
        Guid ancestryId = Guid.NewGuid();
        PopulationEntry<FakeChromosome> entry = Entry(1.0f, ancestryId);
        int generatorCalls = 0;
        GenePool<FakeChromosome> pool = new(r => {
            generatorCalls++;
            return new([], "generated", fitnessModifier: 1.0f);
        });
        pool.Add(entry);

        SequenceRng rng = new();
        for (int i = 0; i < 5; i++) {
            PopulationEntry<FakeChromosome> drawn = pool.Next(rng);
            Assert.That(drawn, Is.SameAs(entry), $"draw {i + 1} should still return the only entry in the pool");
        }

        PopulationEntry<FakeChromosome> sixthDraw = pool.Next(rng);
        Assert.That(sixthDraw, Is.Not.SameAs(entry), "the retired lineage must fall back to a freshly generated entry");
        Assert.That(generatorCalls, Is.EqualTo(1));
    }


    [Test, Parallelizable]
    public void Next_LineageDrawnFourTimes_IsNotYetRemoved() {
        Guid ancestryId = Guid.NewGuid();
        PopulationEntry<FakeChromosome> entry = Entry(1.0f, ancestryId);
        GenePool<FakeChromosome> pool = new(r => new([], "generated", fitnessModifier: 1.0f));
        pool.Add(entry);
        SequenceRng rng = new();

        for (int i = 0; i < 4; i++)
            pool.Next(rng);

        PopulationEntry<FakeChromosome> fifthDraw = pool.Next(rng);
        Assert.That(fifthDraw, Is.SameAs(entry));
    }


    [Test, Parallelizable]
    public void Next_EmptyPool_GeneratesNewEntryWithFreshAncestry() {
        int generatorCalls = 0;
        GenePool<FakeChromosome> pool = new(r => {
            generatorCalls++;
            return new([], "generated", fitnessModifier: 1.0f);
        });
        SequenceRng rng = new();

        PopulationEntry<FakeChromosome> first = pool.Next(rng);
        PopulationEntry<FakeChromosome> second = pool.Next(rng);

        Assert.That(generatorCalls, Is.EqualTo(2));
        Assert.That(first.AncestryId, Is.Not.EqualTo(second.AncestryId));
    }


    [Test, Parallelizable]
    [Description("Independent oracle: hand-computed cumulative FitnessSelector values (1, 3, 6 for fitness 1, 2, 3) predict which entry each of three scripted NextFloat draws selects.")]
    public void Next_MultipleEntries_SelectsByCumulativeFitnessSelector() {
        GenePool<FakeChromosome> pool = new(r => new([], "generated", fitnessModifier: 1.0f));
        PopulationEntry<FakeChromosome> low = Entry(1.0f, Guid.NewGuid());
        PopulationEntry<FakeChromosome> mid = Entry(2.0f, Guid.NewGuid());
        PopulationEntry<FakeChromosome> high = Entry(3.0f, Guid.NewGuid());
        pool.Add(low);
        pool.Add(mid);
        pool.Add(high);

        SequenceRng rng = new() { FloatValues = [0.0f, 0.2f, 0.999f] };

        Assert.That(pool.Next(rng), Is.SameAs(low));
        Assert.That(pool.Next(rng), Is.SameAs(mid));
        Assert.That(pool.Next(rng), Is.SameAs(high));
    }
}
