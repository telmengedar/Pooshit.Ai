using System.Text;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.Evaluation;
using Pooshit.Ai.Serialization;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class NegativePathsTests {

    [Test, Parallelizable]
    public void Population_Constructor_ZeroSize_ThrowsArgumentException() {
        Assert.That(() => new Population<DynamicBOConfiguration>(0, rng => new(["x"], ["y"], rng)),
                    Throws.ArgumentException);
    }


    [Test, Parallelizable]
    public void Population_Constructor_NegativeSize_ThrowsOverflowBeforeGuardCanRun() {
        Assert.That(() => new Population<DynamicBOConfiguration>(-1, rng => new(["x"], ["y"], rng)),
                    Throws.InstanceOf<OverflowException>());
    }


    [Test, Parallelizable]
    public void Population_ArrayConstructor_EmptyArray_ThrowsArgumentException() {
        Assert.That(() => new Population<DynamicBOConfiguration>(Array.Empty<PopulationEntry<DynamicBOConfiguration>>(), rng => new(["x"], ["y"], rng)),
                    Throws.ArgumentException);
    }


    [Test, Parallelizable]
    public void SamplesEvaluator_EmptySampleSet_ReturnsZeroFitnessWithoutThrowing() {
        SamplesEvaluator<DynamicBOConfiguration, DynamicBONet> evaluator = new(Array.Empty<TrainingSample>());
        DynamicBOConfiguration chromosome = new(["x"], ["y"]);

        float fitness = evaluator.EvaluateFitness(chromosome, new SequenceRng(), true);

        Assert.That(fitness, Is.EqualTo(0.0f));
    }


    [Test, Parallelizable]
    public void SamplesEvaluator_SampleOutputKeyNamesUnknownNeuron_ThrowsIndexOutOfRange() {
        SamplesEvaluator<DynamicBOConfiguration, DynamicBONet> evaluator = new([
            new(new float[] { 1.0f }, new Dictionary<string, float> { ["nonexistent"] = 2.0f })
        ]);
        DynamicBOConfiguration chromosome = new(["x"], ["y"]);

        Assert.That(() => evaluator.EvaluateFitness(chromosome, new SequenceRng(), true),
                    Throws.InstanceOf<IndexOutOfRangeException>());
    }


    [Test, Parallelizable]
    public void AiSerialization_Deserialize_UnrecognizedHeader_ThrowsInvalidOperation() {
        using MemoryStream stream = new([1, 2, 3, 4, 5, 6]);
        Assert.That(() => AiSerialization.Deserialize<DynamicBOConfiguration>(stream).ToArray(),
                    Throws.InvalidOperationException);
    }


    [Test, Parallelizable]
    public void AiSerialization_Deserialize_UnexpectedChunkAfterHeader_ThrowsInvalidOperation() {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true)) {
            writer.Write(Encoding.UTF8.GetBytes("P00AI"));
            writer.Write((byte)99);
        }

        stream.Position = 0;
        Assert.That(() => AiSerialization.Deserialize<DynamicBOConfiguration>(stream).ToArray(),
                    Throws.InvalidOperationException);
    }
}
