# Test rules

Six rules against assertions that cannot fail. Full rationale: `docs/architecture/testing-and-measurement.md` §10.1 (DiVoid #9072). Three QA rounds on PR #1 were spent on tests that looked like guards and could not fail — a `Sum` assertion satisfiable by several different multisets, and a test fake that silently discarded the parameter it existed to constrain. These rules exist so the next reviewer does not have to rediscover that the hard way.

## R1 — The sibling-variation rule

Every test that pins an output must have a sibling that varies the input the output is supposed to depend on, and asserts the output moved. An assertion that produces the same verdict for two materially different inputs is provably not measuring that input.

**R4 does not imply R1.** An enum-exhaustive pairwise-distinctness test proves its members differ from each other; it proves nothing about any one of them being a function of its input. A constant response vector can still be unique among its enum siblings. Each enum member still needs its own second probe asserting movement.

Reviewer check: for each pinning assertion, point at its sibling. An enum-exhaustive test is not a substitute sibling for its own members.

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

**The pin must pass under the fix, not merely fail without it.** Before adding the `[Ignore]`, apply the fix locally, run the test, confirm it goes **green**, then revert. A pin verified only red-on-arrival is half-verified — state the two-sided verification in the PR body.

**Fixtures must be fix-tolerant too**, not only the assertion. Where a label map, scripted RNG or slot index encodes the defective path, widen it to cover both paths (a superset map, or a double that does not key on the value the fix moves) so the fix changes the verdict and nothing else. A fixture built only for the defective shape turns "remove the `[Ignore]`" into a `KeyNotFoundException` instead of a pass.

**The trigger is "is this behaviour filed?", not "does this look wrong to me".** Before writing any expectation — including one your test is not directly about — search DiVoid for the mechanic under test. If a `bug`/`task` node describes it, R5 binds, regardless of whether the test's subject *is* that defect. A test about the tournament can still encode a filed defect in its fixture data.

**An `[Ignore]`d intent pin and a tripwire test are complements, not alternatives.** The intent pin asserts the contract wanted — red-if-un-ignored today, green on the fix, never optional. A tripwire documents current defective behaviour — green today, red on the fix — and is acceptable only when: (1) an intent pin exists alongside it; (2) its `[Description]` says explicitly it is expected to go red on the fix, and names the sibling; (3) the defect task's acceptance criteria name both tests and both actions; (4) both directions are verified (pin goes green under the fix, tripwire goes red). **What is still forbidden: a test that depends on a defect without declaring it.** The harm is never the dependency — it is the dependency being undeclared.

Reviewer check: does any assertion's expected value trace to a filed defect? For every `[Ignore]`d pin, apply the referenced fix and confirm green. For every test whose fixture encodes a mechanic with an open defect node, apply that fix and confirm the test survives. For every tripwire, confirm an intent pin exists beside it and both directions are verified.

## R6 — The independent-oracle rule

Prefer an oracle the production path cannot influence. Where a literal is unavoidable, derive it symbolically — never capture it from a test run.

An expected value obtained by running the code and pasting the output is still derived from the code — it wears the costume of an independent expectation while pinning whatever the implementation does, including any error in it. Good oracles, in preference order: a BCL function the code under test does not call, a hand-computed exact rational, a value derived from the specification rather than the implementation.

For a `double` literal pinning `float` arithmetic, the check is mechanical: a `float` widened to `double` leaves 29 low zero bits; a symbolically-derived rational does not. Where the mantissa technique does not apply, ask the implementer how the number was obtained and require the derivation in the return.

Reviewer check: was this expected value derived, or captured? For a `float`-precision literal, check the trailing mantissa bits.
