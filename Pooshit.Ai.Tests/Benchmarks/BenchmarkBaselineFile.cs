namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// locates the committed <c>Benchmarks/baseline.json</c> in the source tree, independent of
/// where the test binaries were built to, so both reading and recording the baseline touch the
/// same file the repository tracks rather than a build-output copy (design #9072 §16 step 19)
/// </summary>
public static class BenchmarkBaselineFile {
    const string ProjectFileName = "Pooshit.Ai.Tests.csproj";

    /// <summary>
    /// full path to the committed baseline file, found by walking up from the running test
    /// binary's directory
    /// </summary>
    public static string Locate() => Locate(AppContext.BaseDirectory);

    /// <summary>
    /// full path to the committed baseline file, found by walking up from <paramref name="searchStart"/>
    /// until a directory containing <c>Pooshit.Ai.Tests.csproj</c> by that exact name is found - not
    /// merely the first <c>.csproj</c> encountered, which could belong to an unrelated project sitting
    /// in an intermediate directory (QA #9388 W3)
    /// </summary>
    internal static string Locate(string searchStart) {
        DirectoryInfo directory = new(searchStart);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, ProjectFileName)))
            directory = directory.Parent;

        if (directory == null)
            throw new InvalidOperationException($"Unable to locate '{ProjectFileName}' by walking up from '{searchStart}'");

        return Path.Combine(directory.FullName, "Benchmarks", "baseline.json");
    }
}
