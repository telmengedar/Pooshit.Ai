using System.Reflection;
using System.Text.RegularExpressions;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class IgnoreLedgerTests {
    static readonly Regex DiVoidReference = new(@"DiVoid #\d+", RegexOptions.Compiled);

    static IEnumerable<string> IgnoreViolations(string ownerDescription, IEnumerable<CustomAttributeData> attributes) {
        foreach (CustomAttributeData attribute in attributes) {
            if (attribute.AttributeType != typeof(IgnoreAttribute))
                continue;

            string reason = attribute.ConstructorArguments.Count > 0
                                 ? attribute.ConstructorArguments[0].Value as string
                                 : null;

            if (reason == null || !DiVoidReference.IsMatch(reason))
                yield return $"{ownerDescription}: [Ignore] reason '{reason}' does not name a 'DiVoid #NNNN' defect";
        }
    }


    [Test, Parallelizable]
    [Description("Closes R5's one weakness (design #9072 §10.1/§16): reflects over this assembly's own [Ignore] attributes, at both method AND fixture (class) level (not production internals - #114) so a skipped test's reason must name a DiVoid defect, keeping 'remove the [Ignore]' a discoverable acceptance criterion.")]
    public void EveryIgnoredTest_ReasonNamesADiVoidDefect() {
        List<string> violations = [];

        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
            violations.AddRange(IgnoreViolations(type.FullName, type.CustomAttributes));

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                violations.AddRange(IgnoreViolations($"{type.FullName}.{method.Name}", method.CustomAttributes));
        }

        Assert.That(violations, Is.Empty, () => string.Join(Environment.NewLine, violations));
    }
}
