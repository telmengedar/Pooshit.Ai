using Pooshit.Ai.Genetics.Mutation;
using Pooshit.Ai.Net.Operations;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class MutationOptionsTests {

    static void AssertBoundarySelection<T>(MutationOptions<T> ladder, double scriptedDraw, T expected) {
        SequenceRng rng = new() { DoubleValues = [scriptedDraw] };
        Assert.That(ladder.SelectItem(rng), Is.EqualTo(expected));
    }


    static MutationEntry<OperationType>[] OperationLadder() => [
        new(OperationType.Multiply, 2.0),
        new(OperationType.Add, 2.0),
        new(OperationType.Div, 4.0)
    ];


    [Test, Parallelizable]
    [TestCase(0.24, OperationType.Multiply)]
    [TestCase(0.25, OperationType.Multiply)]
    [TestCase(0.26, OperationType.Add)]
    [TestCase(0.49, OperationType.Add)]
    [TestCase(0.5, OperationType.Add)]
    [TestCase(0.51, OperationType.Div)]
    public void SelectItem_OperationTypeLadder_ReturnsEntryOfContainingBracket(double scriptedDraw, OperationType expected) {
        AssertBoundarySelection(new OperationTypeOptions(OperationLadder()), scriptedDraw, expected);
    }


    static MutationEntry<AggregateType>[] AggregateLadder() => [
        new(AggregateType.Sum, 2.0),
        new(AggregateType.Average, 2.0),
        new(AggregateType.Min, 4.0)
    ];


    [Test, Parallelizable]
    [TestCase(0.24, AggregateType.Sum)]
    [TestCase(0.25, AggregateType.Sum)]
    [TestCase(0.26, AggregateType.Average)]
    [TestCase(0.49, AggregateType.Average)]
    [TestCase(0.5, AggregateType.Average)]
    [TestCase(0.51, AggregateType.Min)]
    public void SelectItem_AggregateTypeLadder_ReturnsEntryOfContainingBracket(double scriptedDraw, AggregateType expected) {
        AssertBoundarySelection(new AggregateTypeOptions(AggregateLadder()), scriptedDraw, expected);
    }


    static MutationEntry<ActivationFunc>[] ActivationLadder() => [
        new(ActivationFunc.None, 2.0),
        new(ActivationFunc.Sigmoid, 2.0),
        new(ActivationFunc.Tanh, 4.0)
    ];


    [Test, Parallelizable]
    [TestCase(0.24, ActivationFunc.None)]
    [TestCase(0.25, ActivationFunc.None)]
    [TestCase(0.26, ActivationFunc.Sigmoid)]
    [TestCase(0.49, ActivationFunc.Sigmoid)]
    [TestCase(0.5, ActivationFunc.Sigmoid)]
    [TestCase(0.51, ActivationFunc.Tanh)]
    public void SelectItem_ActivationFuncLadder_ReturnsEntryOfContainingBracket(double scriptedDraw, ActivationFunc expected) {
        AssertBoundarySelection(new ActivationFuncOptions(ActivationLadder()), scriptedDraw, expected);
    }


    [Test, Parallelizable]
    public void SelectItem_SingleEntryLadder_ReturnsThatEntryWithoutConsultingRng() {
        OperationTypeOptions ladder = new(new MutationEntry<OperationType>(OperationType.Sub, 1.0));
        SequenceRng rng = new();

        Assert.That(ladder.SelectItem(rng), Is.EqualTo(OperationType.Sub));
    }


    [Test, Parallelizable]
    public void SelectItem_EmptyLadder_ReturnsDefaultWithoutConsultingRng() {
        EmptyDefaultsMutationOptions<OperationType> ladder = new();
        SequenceRng rng = new();

        Assert.That(ladder.SelectItem(rng), Is.EqualTo(default(OperationType)));
    }
}
