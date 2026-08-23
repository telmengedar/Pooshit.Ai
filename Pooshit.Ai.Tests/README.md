# Test rules

Five rules against assertions that cannot fail. Full rationale: `docs/architecture/testing-and-measurement.md` §10.1 (DiVoid #9072). Three QA rounds on PR #1 were spent on tests that looked like guards and could not fail — a `Sum` assertion satisfiable by several different multisets, and a test fake that silently discarded the parameter it existed to constrain. These rules exist so the next reviewer does not have to rediscover that the hard way.

## R1 — The sibling-variation rule

Every test that pins an output must have a sibling that varies the input the output is supposed to depend on, and asserts the output moved. An assertion that produces the same verdict for two materially different inputs is provably not measuring that input.

Reviewer check: for each pinning assertion, point at its sibling.

## R2 — The injective-fixture rule

When the only observable is an aggregate, choose fixture values that make the aggregate injective over the property under test. If you cannot, assert on a richer observable or add a recording double.

Reviewer check: name the pre-image the assertion pins down. If the answer is "some set of values that produces this result", the assertion is not a guard.

## R3 — The total-double rule

A test double throws from every member the test does not deliberately exercise, and records the arguments of every member it does. No silent defaults, ever.

A double may deliberately ignore an input — `FakeNet` is a constant-zero oracle on purpose. Deliberately ignoring is allowed; silently ignoring is not. The difference is recording.

Reviewer check: for each double member, is it throw, or is it record? A third answer is a finding.

## R4 — The enum-exhaustive rule

Tests over enum-driven behaviour enumerate `Enum.GetValues<T>()` rather than listing cases, and assert pairwise-distinct responses wherever the semantics require distinctness. A `default:` case merged into a real enum member gives a new enum value a silent, wrong meaning rather than failing — enumerating the enum makes the omission fail; pairwise distinctness makes the silent aliasing fail too.

Reviewer check: does any test over an enum-driven surface enumerate a hand-written list?

## R5 — The defect-pinning rule

Never encode known-defective behaviour as an expectation. Where the intended contract is known and currently violated, write the assertion to the intended contract and `[Ignore("DiVoid #NNNN")]` it. Removing the `[Ignore]` becomes the acceptance criterion of the fix.

Reviewer check: does any assertion's expected value trace to a filed defect?
