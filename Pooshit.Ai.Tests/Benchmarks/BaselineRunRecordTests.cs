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
    [Description("A baseline recorded before DiVoid #9511 has no NonFiniteGenerations property in its JSON at all. Deserializing it must produce null, not a silently fabricated 0 - the exact failure mode #9511 warns against ('silently defaulting to 0 would fabricate a fact').")]
    public void Deserialize_JsonPredatesNonFiniteGenerationsField_NonFiniteGenerationsIsNull() {
        const string json = """{"ProblemName":"BinOp.MultiplyMinus","Seed":1,"FinalFitness":0.5,"Generations":500}""";

        BaselineRunRecord record = JsonSerializer.Deserialize<BaselineRunRecord>(json);

        Assert.That(record.NonFiniteGenerations, Is.Null);
    }

    [Test, Parallelizable]
    [Description("Sibling to the case above (R1): a baseline that explicitly recorded zero non-finite generations must deserialize to 0, not null - the null/0 distinction this schema exists to preserve must survive a full JSON round-trip, not just construction in memory.")]
    public void Deserialize_JsonRecordsExplicitZero_NonFiniteGenerationsIsZeroNotNull() {
        const string json = """{"ProblemName":"BinOp.MultiplyMinus","Seed":1,"FinalFitness":0.5,"Generations":500,"NonFiniteGenerations":0}""";

        BaselineRunRecord record = JsonSerializer.Deserialize<BaselineRunRecord>(json);

        Assert.That(record.NonFiniteGenerations, Is.Not.Null);
        Assert.That(record.NonFiniteGenerations, Is.EqualTo(0));
    }

    [Test, Parallelizable]
    [Description("A recorded nonzero value round-trips as itself, not as null or as a truncated/boolean-collapsed value - the third point needed to pin the field's shape (null, 0, and a real measurement are three distinct outcomes).")]
    public void Deserialize_JsonRecordsNonZeroValue_NonFiniteGenerationsRoundTrips() {
        const string json = """{"ProblemName":"BinOp.MultiplyMinus","Seed":1,"FinalFitness":0.5,"Generations":500,"NonFiniteGenerations":7}""";

        BaselineRunRecord record = JsonSerializer.Deserialize<BaselineRunRecord>(json);

        Assert.That(record.NonFiniteGenerations, Is.EqualTo(7));
    }
}
