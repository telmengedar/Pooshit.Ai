namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// locates the committed <c>Benchmarks/baseline.json</c> in the source tree, independent of
/// where the test binaries were built to, so both reading and recording the baseline touch the
/// same file the repository tracks rather than a build-output copy (design #9072 §16 step 19)
/// </summary>
public static class BenchmarkBaselineFile {

    /// <summary>
    /// full path to the committed baseline file, found by walking up from the running test
    /// binary's directory until a <c>.csproj</c> is found
    /// </summary>
    public static string Locate() {
        DirectoryInfo directory = new(AppContext.BaseDirectory);
        while (directory != null && directory.GetFiles("*.csproj").Length == 0)
            directory = directory.Parent;

        if (directory == null)
            throw new InvalidOperationException($"Unable to locate the test project directory by walking up from '{AppContext.BaseDirectory}'");

        return Path.Combine(directory.FullName, "Benchmarks", "baseline.json");
    }
}
