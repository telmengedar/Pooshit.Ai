# Test rules

Seven rules against assertions that do not constrain what they appear to — six against assertions that cannot fail (R1–R6), one against assertions that fail correctly while their subject is never reached (R7). Full rationale: `docs/architecture/testing-and-measurement.md` §10.1 (DiVoid #9072). Three QA rounds on PR #1 were spent on tests that looked like guards and could not fail — a `Sum` assertion satisfiable by several different multisets, and a test fake that silently discarded the parameter it existed to constrain. These rules exist so the next reviewer does not have to rediscover that the hard way.

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

**`SequenceRng`'s totality is one-sided, by design.** Over-consuming its script throws; under-consuming it does not, because scripting a superset is exactly what R5's fix-tolerance clause asks for. A mutant that *reduces* the draw count is therefore invisible to script exhaustion alone — when the property under test is how many times the production path drew, assert on the recorded `Bounds`, not on the script running out.

**R3 and R7 are two halves of one instrument, not competitors.** R3 obliges the double to **record**; R7 obliges you to **count what it recorded**. R3's `Bounds` closes the gap where a mutant reduces the draw count; R7 closes the gap where the draws never happened at all.

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

## R7 — The reached-subject rule

An assertion being reachable is not the same as its subject being reached. Where production parks a value on a shared object and reads it back elsewhere, assert at the read, and pin the number of observations the consumer made.

R1–R6 all ask *can this assertion fail?* R7 is orthogonal: the assertion can fail, precisely, on a quantity that had no effect in the fixture's configuration. The instance (DiVoid #10068): a fixture pinned `Train`'s mutation-depth ladder by reading `setup.Mutation.Runs` back off the setup across 140 generations — but ran a population of 2 at `Elitism = 2`, so every entry was an elite, nothing reproduced, and the consumer ran **once in 140 generations**. Green, precise, and measuring nothing. It violated none of R1–R6.

**#9998**'s gene-pool eviction trap is the same category arriving by a different route — there the fixture stops entering its own branch when an ancestry is evicted on its fifth draw. Both are *the assertion no longer reaches its subject*.

Fix direction: observe where production consumes the value — the recorded `NextInt` bound (`SequenceRng.Bounds`, `RecordingRng.Bounds`), the reproduction log, the evaluator's call log — never a setup field the production code wrote. **The instrument is R3's**: R3 obliges the double to record, R7 obliges you to count what it recorded.

Reviewer check: ask the fixture how many times the consumer ran. A test asserting a per-generation quantity over N generations should show N observations, and asserting that count is the whole check.
