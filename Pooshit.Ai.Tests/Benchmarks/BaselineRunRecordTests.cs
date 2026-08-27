using System.Text.Json;

namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// pins how <see cref="BaselineRunRecord"/> deserializes across the schema boundary DiVoid #9511
/// introduced. A baseline recorded before this change has no "NonFiniteGenerations" JSON property
/// at all, and that absence must deserialize to null - a fact ("not measured") - never to 0, which
/// would silently fabricate a measurement that was never taken
/// </summary>
[TestFixture, Parallelizable]
public class BaselineRunRecordTests {

    [Test, Parallelizable]
    [Description("A baseline JSON missing the NonFiniteGenerations property deserializes to null, not a fabricated 0. DiVoid #9511.")]
    public void Deserialize_JsonPredatesNonFiniteGenerationsField_NonFiniteGenerationsIsNull() {
        const string json = """{"ProblemName":"BinOp.MultiplyMinus","Seed":1,"FinalFitness":0.5,"Generations":500}""";

        BaselineRunRecord record = JsonSerializer.Deserialize<BaselineRunRecord>(json);

        Assert.That(record.NonFiniteGenerations, Is.Null);
    }

    [Test, Parallelizable]
    [Description("An explicit JSON 0 deserializes to 0, not null - proving null/0 survive a real round-trip (R1). DiVoid #9511.")]
    public void Deserialize_JsonRecordsExplicitZero_NonFiniteGenerationsIsZeroNotNull() {
        const string json = """{"ProblemName":"BinOp.MultiplyMinus","Seed":1,"FinalFitness":0.5,"Generations":500,"NonFiniteGenerations":0}""";

        BaselineRunRecord record = JsonSerializer.Deserialize<BaselineRunRecord>(json);

        Assert.That(record.NonFiniteGenerations, Is.Not.Null);
        Assert.That(record.NonFiniteGenerations, Is.EqualTo(0));
    }

    [Test, Parallelizable]
    [Description("A recorded nonzero value round-trips as itself, distinguishing it from both null and 0. DiVoid #9511.")]
    public void Deserialize_JsonRecordsNonZeroValue_NonFiniteGenerationsRoundTrips() {
        const string json = """{"ProblemName":"BinOp.MultiplyMinus","Seed":1,"FinalFitness":0.5,"Generations":500,"NonFiniteGenerations":7}""";

        BaselineRunRecord record = JsonSerializer.Deserialize<BaselineRunRecord>(json);

        Assert.That(record.NonFiniteGenerations, Is.EqualTo(7));
    }
}
