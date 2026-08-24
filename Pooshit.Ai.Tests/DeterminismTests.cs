using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.Evaluation;
using Pooshit.Json;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class DeterminismTests {
    const long Seed = 123456789L;
    const long OtherSeed = 987654321L;

    static TrainingSample[] Samples() => [
        new(new { x = 5, y = 2, z = 7 }, new { result = 3 }),
        new(new { x = 3, y = 3, z = 3 }, new { result = 6 }),
        new(new { x = 10, y = 10, z = 2 }, new { result = 98 }),
        new(new { x = 5, y = 5, z = 1 }, new { result = 24 }),
        new(new { x = 1, y = 40, z = 9 }, new { result = 31 })
    ];

    static (float[] Trajectory, float[] FinalFitness, string Winner) Run(long seed) {
        List<float> trajectory = [];
        Rng rng = new(seed);
        Population<DynamicBOConfiguration> population = new(20, r => new(["x", "y", "z"], ["result"], r), rng);
        EvolutionSetup<DynamicBOConfiguration> setup = new() {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>(Samples()),
            Rng = rng,
            Threads = 1,
            Runs = 30,
            TargetFitness = -1.0f,
            AfterRun = (generation, fitness) => trajectory.Add(fitness)
        };

        PopulationEntry<DynamicBOConfiguration> result = population.Train(setup);
        float[] finalFitness = population.Entries.Select(entry => entry.Fitness).ToArray();
        string winner = Json.WriteString(result.Chromosome);
        return (trajectory.ToArray(), finalFitness, winner);
    }

    [Test, Parallelizable]
    public void Train_SameRng_ProducesIdenticalTrajectoryFitnessVectorAndWinner() {
        (float[] Trajectory, float[] FinalFitness, string Winner) runA = Run(Seed);
        (float[] Trajectory, float[] FinalFitness, string Winner) runB = Run(Seed);

        Assert.That(runA.Trajectory, Has.Length.EqualTo(30));
        Assert.That(runB.Trajectory, Is.EqualTo(runA.Trajectory));
        Assert.That(runB.FinalFitness, Is.EqualTo(runA.FinalFitness));
        Assert.That(runB.Winner, Is.EqualTo(runA.Winner));
    }

    [Test, Parallelizable]
    public void Train_DifferentRng_ProducesDifferentTrajectory() {
        (float[] Trajectory, float[] FinalFitness, string Winner) runA = Run(Seed);
        (float[] Trajectory, float[] FinalFitness, string Winner) runC = Run(OtherSeed);

        Assert.That(runC.Trajectory, Is.Not.EqualTo(runA.Trajectory));
    }

    [Test, Parallelizable]
    public void Train_RngSupplied_DrawsFromTheSuppliedInstance() {
        RecordingRng rng = new(new Rng(Seed));
        Population<DynamicBOConfiguration> population = new(20, r => new(["x", "y", "z"], ["result"], r));
        EvolutionSetup<DynamicBOConfiguration> setup = new() {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>(Samples()),
            Rng = rng,
            Threads = 1,
            Runs = 5,
            TargetFitness = -1.0f
        };

        population.Train(setup);

        Assert.That(rng.CallCount, Is.GreaterThan(100));
    }
}
