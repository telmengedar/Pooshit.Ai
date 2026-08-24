namespace NightlyCode.Ai.Tests;

/// <summary>
/// carried by every fake chromosome so a test double (in particular <see cref="StubFitnessEvaluator{T}"/>)
/// can identify a specific instance without reflecting into production internals
/// </summary>
interface ILabelledChromosome {

    /// <summary>
    /// identifying label a test can use to trace this chromosome through selection
    /// </summary>
    string Label { get; }
}
