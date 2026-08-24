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
    public void Next_LineageDrawnFiveTimes_IsRemovedFromPool() {
        Guid ancestryId = Guid.NewGuid();
        PopulationEntry<FakeChromosome> entry = Entry(1.0f, ancestryId);
        int generatorCalls = 0;
        GenePool<FakeChromosome> pool = new(r => {
            generatorCalls++;
            return new([], "generated", fitnessModifier: 1.0f);
        });
        pool.Add(entry);

        // a single-entry pool always returns that entry without consuming the rng
        SequenceRng rng = new();
        for (int i = 0; i < 5; i++) {
            PopulationEntry<FakeChromosome> drawn = pool.Next(rng);
            Assert.That(drawn, Is.SameAs(entry), $"draw {i + 1} should still return the only entry in the pool");
        }

        // the lineage was drawn 5 times and must now be retired; the pool is empty,
        // so Next falls back to generating a brand-new entry
        PopulationEntry<FakeChromosome> sixthDraw = pool.Next(rng);
        Assert.That(sixthDraw, Is.Not.SameAs(entry));
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
    public void Next_MultipleEntries_SelectsByCumulativeFitnessSelector() {
        // three entries with fitness 1, 2, 3 => cumulative selectors 1, 3, 6 (fitnessSum = 6)
        GenePool<FakeChromosome> pool = new(r => new([], "generated", fitnessModifier: 1.0f));
        PopulationEntry<FakeChromosome> low = Entry(1.0f, Guid.NewGuid());
        PopulationEntry<FakeChromosome> mid = Entry(2.0f, Guid.NewGuid());
        PopulationEntry<FakeChromosome> high = Entry(3.0f, Guid.NewGuid());
        pool.Add(low);
        pool.Add(mid);
        pool.Add(high);

        // selectorValue = NextFloat() * fitnessSum(6.0); FirstOrDefault(selector >= value)
        // 0.0   -> selectorValue 0.0 -> first entry whose cumulative selector (1) >= 0.0 -> low
        // 0.2   -> selectorValue 1.2 -> low's selector (1) < 1.2, mid's selector (3) >= 1.2 -> mid
        // 0.999 -> selectorValue 5.994 -> mid's selector (3) < 5.994, high's selector (6) >= 5.994 -> high
        SequenceRng rng = new() { FloatValues = [0.0f, 0.2f, 0.999f] };

        Assert.That(pool.Next(rng), Is.SameAs(low));
        Assert.That(pool.Next(rng), Is.SameAs(mid));
        Assert.That(pool.Next(rng), Is.SameAs(high));
    }
}
