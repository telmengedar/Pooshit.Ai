using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.Evaluation;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class RngGuardTests {

    static EvolutionSetup<DynamicBOConfiguration> Setup(IRng rng, int threads) => new() {
        Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
            new(new { x = 1 }, new { result = 1 })
        ]),
        Rng = rng,
        Threads = threads,
        Runs = 0
    };

    [Test, Parallelizable]
    public void Train_RngSetWithThreadsGreaterThanOne_ThrowsArgumentException() {
        Population<DynamicBOConfiguration> population = new(2, r => new(["x"], ["result"], r));

        Assert.That(() => population.Train(Setup(new Rng(1), 2)),
                    Throws.ArgumentException);
    }

    [Test, Parallelizable]
    public void Train_RngSetWithThreadsEqualToOne_DoesNotThrow() {
        Population<DynamicBOConfiguration> population = new(2, r => new(["x"], ["result"], r));

        Assert.That(() => population.Train(Setup(new Rng(1), 1)),
                    Throws.Nothing);
    }

    [Test, Parallelizable]
    public void Train_ThreadsGreaterThanOneWithoutRng_DoesNotThrow() {
        Population<DynamicBOConfiguration> population = new(2, r => new(["x"], ["result"], r));

        Assert.That(() => population.Train(Setup(null, 2)),
                    Throws.Nothing);
    }
}
