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
    [Description("A negative size is rejected by the documented size guard, not by an OverflowException from allocating the buffer before the guard runs (DiVoid #9054 item 3).")]
    public void Population_Constructor_NegativeSize_ThrowsArgumentException() {
        Assert.That(() => new Population<DynamicBOConfiguration>(-1, rng => new(["x"], ["y"], rng)),
                    Throws.ArgumentException);
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
    [Description("A sample output key naming no neuron is rejected at sample translation with the offending key in the message, not by an out-of-range access deep inside evaluation (DiVoid #9046 defect 2).")]
    public void SamplesEvaluator_SampleOutputKeyNamesUnknownNeuron_ThrowsArgumentExceptionNamingTheKey() {
        SamplesEvaluator<DynamicBOConfiguration, DynamicBONet> evaluator = new([
            new(new float[] { 1.0f }, new Dictionary<string, float> { ["nonexistent"] = 2.0f })
        ]);
        DynamicBOConfiguration chromosome = new(["x"], ["y"]);

        Assert.That(() => evaluator.EvaluateFitness(chromosome, new SequenceRng(), true),
                    Throws.ArgumentException.With.Message.Contains("nonexistent").And.Message.Contains("output"));
    }


    [Test, Parallelizable]
    [Description("A sample input key naming no neuron is rejected at sample translation with the offending key in the message, not by an out-of-range access deep inside evaluation (DiVoid #9046 defect 2).")]
    public void SamplesEvaluator_SampleInputKeyNamesUnknownNeuron_ThrowsArgumentExceptionNamingTheKey() {
        SamplesEvaluator<DynamicBOConfiguration, DynamicBONet> evaluator = new([
            new(new Dictionary<string, float> { ["mistyped"] = 1.0f }, new Dictionary<string, float> { ["y"] = 2.0f })
        ]);
        DynamicBOConfiguration chromosome = new(["x"], ["y"]);

        Assert.That(() => evaluator.EvaluateFitness(chromosome, new SequenceRng(), true),
                    Throws.ArgumentException.With.Message.Contains("mistyped").And.Message.Contains("input"));
    }


    [Test, Parallelizable]
    [Description("The sibling of the two unknown-key tests: the same fixture shape with resolvable keys reaches evaluation and completes, so the rejection above is the key resolving and not the fixture failing earlier.")]
    public void SamplesEvaluator_SampleKeysNameExistingNeurons_TranslatesAndEvaluates() {
        SamplesEvaluator<DynamicBOConfiguration, DynamicBONet> evaluator = new([
            new(new Dictionary<string, float> { ["x"] = 1.0f }, new Dictionary<string, float> { ["y"] = 2.0f })
        ]);
        DynamicBOConfiguration chromosome = new(["x"], ["y"]);

        Assert.That(() => evaluator.EvaluateFitness(chromosome, new SequenceRng(), true), Throws.Nothing);
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
