namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// pins <see cref="BenchmarkBaselineFile.Locate(string)"/> - QA #9388 W4 flagged both the
/// walk-up's failure path and its now-exact-name matching (W3) as untested despite being the only
/// thing standing between a wrong path and a silently corrupted or misread baseline
/// </summary>
[TestFixture, Parallelizable]
public class BenchmarkBaselineFileTests {

    [Test, Parallelizable]
    [Description("Walking up from a directory nested under the project directory must land on Benchmarks/baseline.json next to the real Pooshit.Ai.Tests.csproj, skipping an unrelated .csproj sitting in an intermediate directory along the way (QA #9388 W3).")]
    public void Locate_NestedUnderProjectDirectoryPastAnUnrelatedCsproj_FindsBaselineNextToTheNamedProject() {
        DirectoryInfo root = Directory.CreateTempSubdirectory("pooshit-baseline-locate-");
        try {
            File.WriteAllText(Path.Combine(root.FullName, "Pooshit.Ai.Tests.csproj"), "");
            DirectoryInfo intermediate = Directory.CreateDirectory(Path.Combine(root.FullName, "bin"));
            File.WriteAllText(Path.Combine(intermediate.FullName, "SomeOtherProject.csproj"), "");
            DirectoryInfo searchStart = Directory.CreateDirectory(Path.Combine(intermediate.FullName, "Release", "net9.0"));

            string located = BenchmarkBaselineFile.Locate(searchStart.FullName);

            Assert.That(located, Is.EqualTo(Path.Combine(root.FullName, "Benchmarks", "baseline.json")));
        }
        finally {
            root.Delete(true);
        }
    }

    [Test, Parallelizable]
    [Description("Walking up from a directory with no Pooshit.Ai.Tests.csproj anywhere above it throws instead of silently returning a wrong path.")]
    public void Locate_NoProjectFileAboveSearchStart_ThrowsInvalidOperationException() {
        DirectoryInfo root = Directory.CreateTempSubdirectory("pooshit-baseline-locate-missing-");
        try {
            Assert.That(() => BenchmarkBaselineFile.Locate(root.FullName), Throws.InvalidOperationException);
        }
        finally {
            root.Delete(true);
        }
    }
}
