using System.Reflection;
using System.Text.RegularExpressions;

namespace NightlyCode.Ai.Tests;

/// <summary>
/// closes R5's one weakness (design #9072 §10.1 / §16): a skipped test is easy to forget about.
/// Every <see cref="IgnoreAttribute"/> in this project's own test assembly must name the DiVoid
/// defect it pins, so "remove the [Ignore]" stays a discoverable acceptance criterion rather than
/// a permanently-skipped test nobody remembers to revisit. This reflects over the TEST assembly's
/// own attributes (not into Pooshit.Ai's production internals - #114), which is exactly what a
/// ledger test needs to do.
/// </summary>
[TestFixture, Parallelizable]
public class IgnoreLedgerTests {
    static readonly Regex DiVoidReference = new(@"DiVoid #\d+", RegexOptions.Compiled);

    [Test, Parallelizable]
    public void EveryIgnoredTest_ReasonNamesADiVoidDefect() {
        List<string> violations = [];

        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                foreach (CustomAttributeData attribute in method.CustomAttributes) {
                    if (attribute.AttributeType != typeof(IgnoreAttribute))
                        continue;

                    string reason = attribute.ConstructorArguments.Count > 0
                                         ? attribute.ConstructorArguments[0].Value as string
                                         : null;

                    if (reason == null || !DiVoidReference.IsMatch(reason))
                        violations.Add($"{type.FullName}.{method.Name}: [Ignore] reason '{reason}' does not name a 'DiVoid #NNNN' defect");
                }
            }
        }

        Assert.That(violations, Is.Empty, () => string.Join(Environment.NewLine, violations));
    }
}
