# Architectural Document: Test and Measurement Strategy for Pooshit.Ai

> **Repo path:** `C:\dev\claude\pooshit.ai\docs\architecture\testing-and-measurement.md`
> **DiVoid:** task **#9071** · project **#8948** · map root **#8951**
> **Standards applied as load-bearing:** Design Contracts **#1136** (Pre-Design Checklist audited in §14) · Code Contracts **#114 §0** (KISS / DRY / YAGNI, the bounce rule)

---

## 1. Problem Statement

Toni, verbatim (2026-08-23, DiVoid #9071):

> *"i think that first i would like to setup a set of real tests so we can actually somehow measure what we are doing - currently like you've seen the tests are more of a test without expectations due to the nature of varying results."*

The whole design answers that sentence. Read it carefully and it contains two separate asks welded together by the word *"currently"*:

1. **"a set of real tests"** — assertions that can actually fail. The suite has **4 assertions total**; `CalculatorTests.cs` is 966 lines, 18 `[Test]` methods, **zero** `Assert` calls (#9045). It can only fail on an unhandled exception.
2. **"so we can actually somehow measure what we are doing"** — a way to say whether training is *good*, and whether a change made it *better*. No quantity of unit tests answers that.

And the clause **"due to the nature of varying results"** names the cause of (1) rather than excusing it. Two compounding facts:

- **Training is irreproducible by construction.** `Population.Train` constructs its RNG internally with no seed and no hook; `EvolutionSetup` exposes neither. `LockedRng` gives atomicity but **not** determinism — draw order under `Parallel.ForEach` is scheduler-dependent (#9034).
- **Even given determinism, the outcome is distributional.** "Did training find a good net" is not an equality assertion.

**Success criteria for this design:**

| # | Criterion |
|---|---|
| S1 | A developer can run one command and get a fast, deterministic, strictly-asserting regression suite that fails when behaviour changes. |
| S2 | A developer can run one command and get a number that says how good training currently is, per problem, as a distribution — not a single lucky run. |
| S3 | The two are architecturally separated, so neither is weakened to accommodate the other. |
| S4 | A real defect fix (#9037, #9040, #9041, #9043 …) that legitimately moves training quality produces a **readable diff**, not a red build. |
| S5 | The dominant local failure mode — *assertions that cannot fail* — is made structurally harder, not merely discouraged. |

**The counter-goal, stated first because it governs every other choice:** a suite that goes red every time a real fix lands will be switched off, and a suite that is switched off is worth less than no suite at all.

---

## 2. Scope & Non-Scope

### In scope

- The **test-layer architecture**: which question each layer answers, what drives it, what it asserts.
- The **determinism seam**: its shape, its guarantees, and the precise production change it requires.
- The **measurement / benchmark approach**: problem set, seed set, recorded metrics, baseline lifecycle.
- The relationship between the two layers, and the **phasing** into PR-sized units.
- Structural countermeasures against unfailable assertions (§10.1).

### Explicitly out of scope

- **Implementing any of it.** This document is the blueprint; John implements.
- **Fixing any referenced defect.** #9037 (non-finite fitness), #9038 (the RNG defect cluster), #9039 (stale net state), #9040 (splice invariant), #9041 (complexity-penalty direction), #9042 (lineage exhaustion), #9043 (diversity collapse), #9046 (Sum scale), #9054 (fresh-blood off-by-one) are named here **only** as things the design must survive, never as things it repairs.
- **Parallel determinism.** Deliberately decided against in §11.2, with reasoning. Not deferred silently — rejected on the record.
- **Any change to `LockedRng`.** The seam is designed so that file is not touched. Its duplication defect stays with #9038.
- **The dead data fixtures** (`testmodel.json`, `xyzmodel.json`, `excellence_broker_samples.json`). They belong to #9045. Raised once in §15 and otherwise left alone.
- **CI configuration.** There is no CI pipeline in this repo today; "the regression lane" means `dotnet test` on a developer machine. No pipeline is designed, and none is needed for any decision here.

---

## 3. Assumptions & Constraints

| # | Assumption / constraint | Confidence | Consequence if wrong |
|---|---|---|---|
| A1 | NUnit 3.13 + `dotnet test` remains the only test runner; no new test framework, no new test project. | High — verified in `Pooshit.Ai.Tests.csproj`. | A different runner changes only lane-selection syntax, not the architecture. |
| A2 | `float` arithmetic is bit-identical **within one process on one machine**, and not guaranteed across machines/runtimes (`MathF.Sin`, `MathF.Tanh`, `AMath.Power`, FMA contraction). | High. | Drives D3 (§11.3): no committed literal fitness values, ever. |
| A3 | At `Threads == 1` the whole training path is single-threaded once the one `Parallel.ForEach` in `Evolve` is branched (§8.1 P3). | High — verified by reading `Population.cs`. Hazard inventory in §10.2. | If a hidden concurrency source remains, the determinism test fails immediately and loudly. That is the correct outcome. |
| A4 | `Guid.NewGuid()` for `AncestryId` does not affect run outcome. Ancestry ids are used only for dictionary keying and equality filtering in `GenePool` — never for ordering or selection. | High — verified in `GenePool.cs` and `Population.cs`. | If wrong, determinism is unreachable without a `Guid` seam. The determinism test detects this on its first run. |
| A5 | Benchmarks run on Toni's machine, on demand, by a human. Not scheduled, not gated, not remote. | High — no CI exists. | A future scheduled run would need a machine-pinned baseline; not designed for. |
| A6 | Two nets of the same family with the same chromosome compute identical outputs, so serialization round-trips can be asserted behaviourally, not just structurally. | Medium — `AiSerializationTests` today asserts only JSON-string equality and blanks input-neuron `Activation`/`Aggregate` before comparing (#9045). | If behavioural round-trip fails on arrival, that is a **finding**, filed as a bug, and R5 (§10.1) applies. |

**Hard constraint (design-shaping):** the regression lane must be fast enough that a developer runs it without thinking. Several existing tests run 5000 generations. Anything slower than a few seconds gets skipped, and a skipped suite is a dead suite.

---

## 4. Architectural Overview

Two lanes. They share a project, a runner and a set of test doubles, and **nothing else**. The separation is the whole design; conflating them is precisely how the current suite ended up asserting nothing.

```
                         Pooshit.Ai.Tests  (one project, one runner)
    +---------------------------------------------------------------------------+
    |                                                                           |
    |  LANE 1 - REGRESSION                     LANE 2 - MEASUREMENT             |
    |  "did behaviour change?"                 "is training any good, and       |
    |                                           did this change help?"          |
    |  runs on: every `dotnet test`            runs on: --filter Category=      |
    |  default lane, no filter                          Benchmark  ([Explicit]) |
    |  budget: seconds                         budget: minutes                  |
    |  determinism: total                      determinism: per (problem,seed)  |
    |  assertions: exact, strict               assertions: invariants strict;   |
    |                                                      quality REPORTED     |
    |                                                                           |
    |  +-------------------------+             +------------------------------+ |
    |  | 1a  Pure units          |             |  BenchmarkHarness            | |
    |  |  NMath . AMath          |             |   problems x seeds -> results| |
    |  |  EnumerableExtensions   |             +-----------+------------------+ |
    |  |  MutationOptions ladders|                         |                    |
    |  |  StructureHash          |              +----------+----------+         |
    |  |  serialization          |              |                     |         |
    |  +-------------------------+        +-----v------+      +-------v-------+ |
    |  +-------------------------+        |  Compare   |      |RecordBaseline | |
    |  | 1b  Genetics mechanics  |        |  vs commit-|      |  overwrites   | |
    |  |  Population via Train   |        |  ted base- |      |  baseline.json| |
    |  |  driven by              |        |  line ->   |      |  ([Explicit]) | |
    |  |  StubFitnessEvaluator   |        |  PRINTS a  |      +-------+-------+ |
    |  +-------------------------+        |  table     |              |         |
    |  +-------------------------+        +-----+------+              |         |
    |  | 1c  Determinism pair    |              |                     |         |
    |  |  same Rng => same run   |              +----------+----------+         |
    |  |  diff Rng => diff run   |                         v                    |
    |  |  (real end-to-end       |            Benchmarks/baseline.json          |
    |  |   training, small)      |            (committed; moves in a PR diff,   |
    |  +-------------------------+             never as a red build)            |
    +---------------------------------------------------------------------------+
                                  |
                       depends on v  ONE production change
    +---------------------------------------------------------------------------+
    |  Pooshit.Ai - the determinism seam                                        |
    |   * EvolutionSetup<T>.Rng : IRng  (new property, null = today's behaviour) |
    |   * Population.Train      : honour setup.Rng; throw if set with Threads>1  |
    |   * Population.Evolve     : re-score serially when Threads == 1           |
    |   nothing else. Rng, LockedRng, IRng, GenePool, SamplesEvaluator untouched.|
    +---------------------------------------------------------------------------+
```

**The load-bearing asymmetry:** Lane 1 is strict *and* runs always, which it can afford because it is deterministic. Lane 2 is strict *and* opt-in, which is why it can afford to be strict without ever breaking a build. Strictness in an on-demand diagnostic lane is free. Strictness in an always-on lane is only affordable when the thing under test is deterministic. Every allocation of a test to a lane follows from that one sentence.

---

## 5. Components & Responsibilities

Existing components keep their real names. New components are named for what they are.

### 5.1 Production side

| Component | Owns | Does NOT own |
|---|---|---|
| `EvolutionSetup<T>` | The complete description of one training run, now **including the random stream** it draws from. | Constructing the stream. Deciding whether the stream is reproducible. |
| `Population<T>.Train` | Executing a run against the supplied setup; honouring `setup.Rng` when present; refusing the combination that cannot deliver what it appears to promise. | Seeding. Reproducibility across processes. Any statistical property of the stream. |
| `Rng`, `LockedRng`, `IRng` | **Unchanged.** | — |

### 5.2 Test doubles (test project, flat alongside the existing ones)

The project already has `FakeChromosome`, `FakeNet`, `SequenceRng`. **Extend these; do not add parallel types** (Design Contracts §1 DRY, Code Contracts §5.4 — a `StubChromosome` next to a `FakeChromosome` is the mirror-type anti-pattern).

| Double | Responsibility | Must NOT do |
|---|---|---|
| `FakeChromosome` (extend) | Be an `IChromosome<FakeChromosome>` whose `StructureHash()` and `FitnessModifier` are **supplied per instance**, not hardcoded. Carry an identifying label so a test can trace it through selection. | Keep `FitnessModifier => 0.0f`. That value is used as a **divisor** in `Population.Evolve` — every breeding weight becomes `Infinity`. As it stands the type cannot be used in any `Population` test. |
| `MutatingFakeChromosome` | Implement `IMutatingChromosome<…>`; record every `Mutate(rng, range)` call — receiver label, range, call ordinal — and return a new labelled instance. Selects the `Mutate` reproduction strategy. | Mutate the receiver. Reproduction must be pure and thread-safe (#9029). |
| `CrossingFakeChromosome` | Implement `ICrossChromosome<…>`; record every `Cross(other, CrossSetup)` call including the `MutateChance` / `MutateRate` / `MutateRange` it received. Selects the `Cross` strategy. | — |
| `AmbidextrousFakeChromosome` | Implement **both** interfaces. Exists for exactly one test: that `Population`'s constructor binds `Cross` and never mutates (#9029). | Be reused anywhere else. |
| `FakeNet` (harden) | Be a deterministic oracle net. Record `SetInputValues` / indexer writes / `Update` arguments so a test can assert what it received. Throw from `this[string]` — the evaluator resolves by index and never reads by name. | Return a silent default from any member that a test might believe it constrains. This is the exact shape that burned three QA rounds. |
| `SequenceRng` (extend) | Script `NextFloat`, `NextDouble`, `NextLong`, `NextInt()` the same way `NextInt(max)` is already scripted: record what was asked, validate the scripted value against the requested bound, throw on exhaustion. | Return `0` / `default` for an unscripted member. Every unscripted member throws `NotSupportedException` — the current file gets this exactly right and is the template. |
| `StubFitnessEvaluator<T>` (new) | Return a **caller-specified fitness per chromosome label**; record every `(chromosome, fullSet)` call in order. Throw on a label it was not given a value for. | Approximate. Interpolate. Return a default for an unknown chromosome — that is how a mechanics test silently stops measuring selection. |

`StubFitnessEvaluator<T>` is the single highest-leverage new component in this design: it makes the entire genetics layer — elitism, dedup, ordering, breeding weights, gene-pool draws, rivalism, fresh blood — testable with **zero** nets, **zero** samples, **zero** floating-point noise and **zero** dependence on the determinism seam.

### 5.3 Measurement side (test project, `Benchmarks/`)

| Component | Responsibility | Does NOT own |
|---|---|---|
| `BenchmarkProblem` | One named, self-contained training problem: chromosome family, net type, sample set, setup template, `TargetFitness`. **Constructs a fresh `EvolutionSetup`, a fresh `SamplesEvaluator` and a fresh `Population` per run** (see hazards H7/H8 in §10.2 — `Train` mutates its setup, and the evaluator caches indexed samples and pools nets). | Seeds. Baselines. Reporting. |
| `BenchmarkHarness` | Execute every (problem, seed) pair, each at `Threads = 1`, parallelising **across pairs**. Produce one run result per pair. | Judging quality. Asserting anything. |
| `BenchmarkComparison` (the `[Explicit]` test) | Load the committed baseline, run the harness, assert the two **invariants** (§11.3), print the comparison table. | Asserting that quality did not regress. |
| `RecordBaseline` (an `[Explicit]` test) | Run the harness and overwrite `Benchmarks/baseline.json` in the source tree. | Running as part of any other lane. |
| `Benchmarks/baseline.json` | The last recorded measurement, with provenance (`recordedAt`, `commit`, `note`). Committed. | Being authoritative about correctness. It is a **record**, not an expectation. |

---

## 6. Interactions & Data Flow

### 6.1 Lane 1c — the determinism pair (the default-lane end-to-end test)

```
  test
   |  construct Rng(SEED)  -------------------------+
   |  construct Population(20, generator, thatRng)   |  RUN A
   |  construct EvolutionSetup { Rng = thatRng,      |
   |      Evaluator = fresh SamplesEvaluator,        |
   |      Threads = 1, Runs = 30,                    |
   |      AfterRun = (gen, fit) => trajectoryA.Add } |
   |  Train  ----------------------------------------+
   |      -> trajectoryA : float[30]
   |      -> final Entries[*].Fitness : float[20]
   |      -> serialized Entries[0].Chromosome : string
   |
   |  repeat the entire construction with Rng(SEED)   -- RUN B
   |
   +- ASSERT trajectoryA == trajectoryB               (exact, element-wise)
   +- ASSERT final fitness vector A == B              (exact, element-wise)
   +- ASSERT serialized winner A == B                 (exact string)
   |
   |  repeat once more with Rng(OTHER_SEED)           -- RUN C
   +- ASSERT trajectoryA != trajectoryC               (the sibling; see R1)
```

Three properties make this test strong:

- **It compares two live runs, never a committed literal.** It therefore cannot rot, cannot be machine-specific, and survives every legitimate change to training behaviour — including all of #9037/#9040/#9041/#9043. It asserts *reproducibility*, which is invariant, not *quality*, which is not.
- **The trajectory, not just the endpoint.** `AfterRun` already delivers `(generation, leaderFitness)`. Comparing 30 floats instead of 1 makes an accidental match impossible and localises a divergence to a generation, which is diagnostic gold when it fires.
- **The inequality sibling is mandatory.** A `Train` that ignored `setup.Rng` *and* was itself internally deterministic (a fixed constant seed) would pass the equality assertions and fail nothing at all. Run C is what closes that hole. See R1 in §10.1.

### 6.2 Lane 1b — mechanics, driven through `Train`

`Evolve`, `Cross`, `Mutate` and `GetOrderNumber` are private. The only public surface is `Train`. This is not an obstacle; it is a constraint that produces better tests, because it forces assertions onto observable outcomes rather than internals.

```
  test builds PopulationEntry<FakeChromosome>[] by hand
        (labels, StructureHash values, FitnessModifier values chosen per case)
   |
   +- Population(entries, generator)      <- the array constructor: no RNG involved
   |
   +- EvolutionSetup { Evaluator = StubFitnessEvaluator (label -> fitness),
   |                   Rng      = SequenceRng (fully scripted),
   |                   Runs = 1, Threads = 1, Elitism = k, Rivalism = r }
   |
   +- Train
   |
   +- observe:
        * population.Entries - order, and **reference identity** of elite entries
        * StubFitnessEvaluator's recorded call log - which chromosomes were scored,
          in what order, with which fullSet flag
        * MutatingFakeChromosome's recorded Mutate log - which parents were drawn,
          how many times, at what range
        * CrossingFakeChromosome's recorded CrossSetup - the exact mutation
          parameters each slot received
```

With `setup.Rng` scripted, **selection becomes fully determined and therefore assertable**. This is not a side benefit of the seam's shape; it is the second reason for choosing that shape (§11.1).

### 6.3 Lane 2 — a benchmark cycle

```
  BenchmarkComparison
   |
   +- read Benchmarks/baseline.json      (problems x seeds -> fitness, generations)
   |
   +- for each problem x seed, in parallel across pairs:
   |       fresh EvolutionSetup, fresh SamplesEvaluator, fresh Population,
   |       Rng(seed), Threads = 1
   |       -> Train -> (finalFitness, generationsExecuted)
   |
   +- ASSERT  every finalFitness is finite and >= 0             (invariant I1)
   +- ASSERT  every finalFitness <= that run's generation-0 best (invariant I2)
   |
   +- PRINT   per problem:
                per-seed paired table   baseline | current | delta
                distribution summary    median / min / max, solved-count
                paired verdict          "improved on 5/8 seeds, regressed on 2"
```

Nothing in that flow asserts that the median improved, or that it did not regress. §11.3 explains why, and what makes I1 and I2 real rather than decorative.

---

## 7. Data Model (Conceptual)

Three entities. One file.

| Entity | Identity | Attributes | Owned by |
|---|---|---|---|
| **Benchmark problem** | `Name` (stable string, e.g. `BinOp.MultiplyMinus`) | net family, chromosome generator, sample set, setup template, target fitness | the harness, in code — **not** in the baseline file |
| **Run result** | (`ProblemName`, `Seed`) | `FinalFitness` (the full-set re-score `Train` returns), `Generations` (how many generations executed before target or exhaustion) | the harness |
| **Baseline** | the file | `recordedAt` (date), `commit` (git sha), `note` (free text: *why* this recording was taken), plus the complete set of run results | the repo, via `RecordBaseline` |

**Deliberately not recorded: wall-clock time.** It is machine-dependent, so baselining it would make the baseline machine-specific and permanently "moved". `Generations` is the machine-independent proxy for convergence speed and is fully deterministic under a fixed seed. (Design Contracts §3: a number that cannot be compared across the environments it is recorded in is not a measurement.)

**Deliberately not modelled: a format-version field.** One file, one reader, changed together. YAGNI.

**`note` and `commit` earn their keep by a named consumer:** the human reading the PR diff when a fix moves the baseline. They are what converts *"the baseline changed"* into *"the baseline changed because #9037's guard landed"*. That is the S4 mechanism, and it is two strings.

**Relationships:** a baseline holds N problems × M seeds run results. Problem *definitions* live in code and are referenced from the file only by name — so a problem whose samples change gets a new name, and an unmatched name is **reported** (not asserted) as "present in baseline, absent from harness" or vice versa.

---

## 8. Contracts & Interfaces (Abstract)

### 8.1 The production change — complete and exhaustive

Three edits. Nothing else in `Pooshit.Ai` is touched.

| # | Surface | Change | Semantics |
|---|---|---|---|
| P1 | `EvolutionSetup<T>` | Add one property: an `IRng` named `Rng`, get/set, default `null`. | `null` — `Train` behaves exactly as today (constructs its own generator). Non-`null` — `Train` draws every random value of the run from this instance. Its XML doc must state: *supplying this makes a run reproducible only if the `Population` was also constructed from a seeded generator, and only at `Threads == 1`.* |
| P2 | `Population<T>.Train` | Where it currently constructs `LockedRng` or `Rng`, prefer `setup.Rng` when non-`null`. **Before** that, throw `ArgumentException` when `setup.Rng` is non-`null` **and** `setup.Threads > 1`. | The guard is the contract made executable. It refuses the only combination in which the property looks like it promises reproducibility and cannot deliver it. The message must say why and what to do: *reproducible runs require `Threads = 1`; leave `Rng` unset to run threaded.* |
| P3 | `Population<T>.Evolve` | The re-score `Parallel.ForEach` over `trainingBuffer` becomes a plain sequential loop when `setup.Threads == 1`, keeping `Parallel.ForEach` otherwise. | **No behavioural change.** With `MaxDegreeOfParallelism = 1` this loop is already effectively serial; the branch turns "effectively" into "contractually", so the evaluator's draw order is source order. It is also a DRY alignment: `Train` (lines 230-234) and `Mutate` (lines 167-175) already branch on `Threads` exactly this way; `Evolve`'s re-score is the one place that does not. |

**Invariants the seam establishes:**

- `Threads == 1` **and** `setup.Rng` supplied **and** the `Population` built from a seeded generator ⇒ the run is a pure function of (initial entries, setup, stream). Same inputs, same trajectory, same winner, byte-for-byte, within one process.
- `Threads > 1` ⇒ no reproducibility is offered, and the API refuses to pretend otherwise.
- `setup.Rng == null` ⇒ byte-for-byte the behaviour shipped today. Every existing caller, including every `CalculatorTests` demo, is unaffected.

**What the seam explicitly does not do:** it does not improve `Rng`'s stream, does not deduplicate `LockedRng`, does not touch `IRng`. Every one of those is #9038, and #9038 landing later will *move the baseline* — which §11.3 is built to absorb.

### 8.2 Test-double contracts (abstract)

| Double | Input contract | Output / recording contract | Failure contract |
|---|---|---|---|
| `StubFitnessEvaluator<T>` | A map from chromosome label to fitness, supplied at construction. | An ordered log of `(label, fullSet)` for every call, readable by the test. | Throws on a label absent from the map. Never returns a default. |
| `SequenceRng` | An ordered script per drawing method. | An ordered log of `(method, requestedBound)` for every call. | Throws on an unscripted method, on script exhaustion, and on a scripted value outside the requested bound. **Under-consumption is deliberately silent** — fixtures legitimately script a superset (R5's fix-tolerance clause) — so a property about *how many* draws happened must be asserted explicitly, off the recorded bounds. |
| `MutatingFakeChromosome` / `CrossingFakeChromosome` | Constructed with a label. | An ordered log of reproduction calls with every parameter received. | Returns a **new** instance always; never mutates the receiver. |
| `FakeNet` | Constructed from a `FakeChromosome`. | Records `SetInputValues` arrays, indexer writes, and `Update` arguments. Returns a value determined only by what the test configured. | Throws from `this[string]`. |

### 8.3 The benchmark harness contract

| Aspect | Contract |
|---|---|
| Input | The problem set (code) + the seed set (code) + the committed baseline (file). |
| Isolation | A fresh `EvolutionSetup`, a fresh `SamplesEvaluator` and a fresh `Population` per (problem, seed). Non-negotiable — see hazards H7 and H8 in §10.2. |
| Threading | Every individual run is `Threads = 1`. Parallelism is across (problem, seed) pairs only. |
| Output | An in-memory result set; a printed comparison table; and — from `RecordBaseline` only — the rewritten baseline file. |
| Assertions | I1 and I2 (§11.3) only. |

---

## 9. Which tests exist, and in which lane

This section is the work inventory. It specifies *what must be asserted*, not how to write the assertion.

### 9.1 Lane 1a — pure units (no production change needed; buildable today)

| Target | What is asserted | Structural rule applied |
|---|---|---|
| `NMath.Compute` | For a fixed probe pair, **every** `OperationType` value produces a result, enumerated from `Enum.GetValues`, and the results are **pairwise distinct**. Plus per-operation exact expected values. | R4. Catches a new enum member falling silently into `default: case Multiply:`. |
| `NMath.Activation` | For a fixed probe **vector** (a negative, a small negative, a small positive, a large positive), every `ActivationFunc` produces a distinct **response vector**. Single-probe distinctness is impossible — `None`, `ReLU` and `LeakyReLU` all map 0.5 to 0.5. Plus the universal non-finite → 0 guard: any func whose raw result is NaN/±∞ returns exactly 0. | R4, and R2 (a single probe is a lossy observable; the vector restores injectivity). |
| `NMath.Aggregate` | Every `AggregateType` over a fixed multiset produces its distinct documented value. Plus the empty-input behaviour of each, explicitly — `Median` on empty, `Min`/`Max` on empty, `AverageToMax` on empty. | R4. |
| `AMath.InverseSquareRoot`, `AMath.Power` | Accuracy bands over a probe set that includes negatives, zero, fractional and large arguments — not only the well-behaved positives the current 5 cases use. | — |
| `EnumerableExtensions.RandomSample` | Draws **without replacement** (the result is a set), respects the requested count, clamps to source length, returns empty for non-positive counts, and — the one that matters — **never writes to the source list**: the source is compared element-wise before and after. Plus the displaced-index revisit path, driven by `SequenceRng`. | R2: assert on the drawn multiset, not on its sum. |
| `EnumerableExtensions.RandomItem` | The selection index equals the drawn value; an empty source returns default. | — |
| `MutationOptions.SelectItem` ladders (`OperationTypeOptions`, `AggregateTypeOptions`, `ActivationFuncOptions`) | Given a scripted `NextDouble`, the returned item is exactly the ladder entry whose cumulative-weight bracket contains the draw — asserted at each bracket boundary, both sides. Plus the single-entry and empty-entry short circuits. | R2 — the cumulative-weight ladder is the pre-image; assert on the boundaries, not on a frequency histogram. |
| `StructureHash` (`DynamicBOConfiguration`, `DynamicFFConfiguration`) | Two structurally identical chromosomes with different weights hash equal; two structurally different chromosomes hash unequal. **The known limitation** — that neuron configuration (aggregate/activation) is ignored (#9043) — is written as the intended contract and `[Ignore]`d per R5. | R1 (the equal/unequal pair is the sibling), R5. |
| `AiSerialization` round-trip | Round-trip a trained chromosome and assert the deserialised chromosome **computes the same outputs** for a fixed input battery — not merely that the JSON strings match. The existing tests blank input-neuron `Activation`/`Aggregate` before comparing, which normalises around the lossy part of the round trip rather than covering it. | R2 — a JSON string is a lossy observable of "did the net survive". |
| Negative paths | Invalid population size; empty training sample set; a sample key naming a neuron that does not exist (rejected at sample translation with an `ArgumentException` naming the offending key, on the input path and the output path alike — #9046 defect 2, closed); a malformed serialised stream. | — |

### 9.2 Lane 1b — genetics mechanics (needs the seam only for the scripted-`IRng` cases)

| Target | What is asserted |
|---|---|
| Reproduction-strategy binding | A chromosome implementing only `IMutatingChromosome` mutates; one implementing only `ICrossChromosome` crosses; one implementing **both** crosses and never mutates (`AmbidextrousFakeChromosome`); one implementing neither throws at construction. |
| Elitism | With `Elitism = k` and `k+n` structurally distinct non-negative entries, exactly the best `k` survive into the next generation **by reference identity**. |
| Structure-hash deduplication | With duplicate `StructureHash` values among the leaders, weight-variants of one topology cannot occupy more than one elite slot; the elite band is correspondingly shorter. |
| Negative-fitness exclusion | An entry with negative fitness is excluded from elitism **and** from the gene pool, and sorts to the back (`GetOrderNumber` returns `float.MaxValue`). |
| Ordering | The post-generation `Entries` array is ascending by fitness, negatives last. |
| Breeding-weight shape | With hand-chosen fitness values and `FitnessModifier`s, and a scripted `IRng` driving `GenePool.Next`, the parent drawn for each slot is the one the cumulative-selector arithmetic predicts. This is the only way to assert the roulette without making the pool public. |
| Gene-pool ancestry eviction | A lineage drawn 5 times is removed from the pool; subsequent draws cannot return it. Written to the intended contract; if the intended retirement count is disputed, R5 applies. |
| Fresh-blood band (`Mutate` strategy) | With a `Generator` returning a marked chromosome, the number of slots carrying the marker equals `Elitism`. Held since #9054 item 2 landed; the band is `i >= trainingBuffer.Length - setup.Elitism` and the R5 pin runs un-`[Ignore]`d. |
| Rivalism | With `Rivalism = r`, exactly `r` candidates are evaluated per slot (assert via the stub evaluator's call log) and the best is kept. Each of the `r` rivals mutates the parent itself, and draws its **own** mutation depth from `Mutation.Runs` — asserted as `r` recorded draws of that bound per slot, and as the per-rival chain shape in the reproduction log (#9931, #9936). |
| Adaptive mutation escalation | When the leader's structure hash is unchanged across generations, `setup.Mutation.Runs` escalates on the documented schedule; when it changes, it resets to 1. Observable directly on the setup object — which is also the test that documents `Train`'s in/out mutation of its own setup. |
| Early exit on `TargetFitness` | With a stub returning a fitness at or below target, `Train` stops after the generation that reached it. Assert via the `AfterRun` invocation count. |
| The seam's guard | `setup.Rng` set together with `Threads > 1` throws; either alone does not. |

### 9.3 Lane 1c — the determinism pair

As specified in §6.1. Small population, few generations, tiny sample set — the whole pair must cost well under a second, because it runs on every `dotnet test`.

It is also the lane's **only** end-to-end exercise of the real net families. That is deliberate and sufficient: it constructs, trains, evolves, evaluates, sorts, re-scores and serialises real `DynamicBO`/`DynamicFF` chromosomes, and unlike the 18 `CalculatorTests` methods it effectively replaces, it asserts.

### 9.4 Lane hygiene, same phase

`CalculatorTests` becomes `[Explicit, Category("Demo")]` at fixture level. It is a demo gallery and a smoke test (#9045) — keeping it is fine, running 5000-generation trainings in the default lane is not. This is a prerequisite for the "fast enough that a developer runs it without thinking" constraint in §3, not a cleanup nicety.

---

## 10. Cross-Cutting Concerns

### 10.1 The six rules against assertions that cannot fail

This is the most valuable part of this document, measured by what has actually cost time on this repo. Three QA rounds on PR #1 were spent on tests that *looked* like guards and could not fail: a `Sum` assertion satisfiable by several different multisets, and a fake that silently discarded the parameter it was meant to constrain. The problem is not test count. It is that **an assertion's inability to fail is invisible at review time.** Reviewer vigilance has already been tried; it caught these three times at a cost of three rounds.

Each rule below converts an invisible property into a visible artefact — something a reviewer can see the *absence* of.

---

**R1 — The sibling-variation rule.**
*Every test that pins an output must have a sibling that varies the input the output is supposed to depend on, and asserts the output moved.*

An assertion that produces the same verdict for two materially different inputs is provably not measuring that input. The canonical instance is the determinism pair: `same Rng ⇒ same trajectory` alone would pass against a `Train` that ignored `setup.Rng` and used a fixed internal constant. `different Rng ⇒ different trajectory` closes it. The two together are unfakeable.

**Sharpened 2026-08-23 (QA #9080), by the very PR that introduced this rule: R4 does not imply R1.** Five activation tests — `Sigmoid`, `Sin`, `Tanh`, `Swish`, `Sqrt` — pinned one output at one probe. Replacing any of those production bodies with an input-independent constant left the entire 122-test lane green. The non-obvious part: R4's enum-exhaustive pairwise-distinctness check cannot rescue them, because a constant response vector such as `[0.5, 0.5, 0.5, 0.5]` is still *unique* among the thirteen activations — R4 proves the members differ from **each other**, not that any one of them is a function of its **input**. The two rules cover orthogonal properties. **Amendment: an enum-exhaustive distinctness test does not satisfy the sibling-variation requirement for its members — each member still needs its own second probe asserting movement.** The control that makes this concrete: `ReLU` carried two `[TestCase]`s and its constant-mutant died; the five single-probe members survived.

Reviewer check: *for each pinning assertion, point at its sibling.* A missing sibling is visible. An enum-exhaustive test is not a substitute sibling for its own members.

---

**R2 — The injective-fixture rule.**
*When the only observable is an aggregate, choose fixture values that make the aggregate injective over the property under test. If you cannot, assert on a richer observable or add a recording double.*

This is the `Sum` bug stated as a rule. `Sum` over an arbitrary multiset is many-to-one; `Sum` over distinct powers of two is a bijection onto bit sets — which is exactly why the existing `SamplesEvaluatorTests` fixture is right, and the reason it is right should be written down rather than rediscovered. Where no injective fixture exists (activation functions at a single probe), widen the observable until it is injective (a probe *vector*).

Reviewer check: *name the pre-image the assertion pins down.* If the answer is "some set of values that sums to 25", the assertion is not a guard.

---

**R3 — The total-double rule.**
*A test double throws from every member the test does not deliberately exercise, and records the arguments of every member it does. No silent defaults, ever.*

`SequenceRng` already embodies this and is the template: unscripted methods throw `NotSupportedException`, `NextInt(max)` records the bound it was asked for, and a scripted value outside that bound throws rather than being quietly clamped. **Its totality is one-sided, by design:** over-consumption throws, under-consumption does not, because a fixture that scripts a superset is the pattern R5's fix-tolerance clause asks for. The recording half is what closes the gap — a mutant that *reduces* the draw count is invisible unless a test asserts on `Bounds`, which is the instrument to reach for whenever the property under test is how many times the production path drew (#9936). `FakeNet` is the counter-example: `this[string name] => 0.0f`, `SetInputValues` discards, `Update` discards. That shape is exactly how a test stops measuring what it claims to.

The rule has a subtlety worth stating, because a naive reading forbids a legitimate pattern: a double may *deliberately* ignore an input — `FakeNet` is a constant-zero oracle on purpose, so the evaluator's distance equals the expected value. **Deliberately ignoring is allowed; silently ignoring is not.** The difference is recording. If the double records what it was handed, a test can assert on it and a reviewer can see the value was observed rather than dropped.

Reviewer check: *for each double member, is it throw, or is it record?* A third answer is a finding.

---

**R4 — The enum-exhaustive rule.**
*Tests over enum-driven behaviour enumerate `Enum.GetValues<T>()` rather than listing cases, and assert pairwise-distinct responses wherever the semantics require distinctness.*

`NMath.Compute` and `NMath.Activation` both switch with `default:` merged into a real case (`default: case OperationType.Multiply:` and `default: case ActivationFunc.None:`). A new enum member therefore acquires a silent, wrong meaning rather than failing. Enumerating the enum makes the omission fail; pairwise distinctness makes the *silent aliasing* fail too.

Reviewer check: *does any test over an enum-driven surface enumerate a hand-written list?*

---

**R5 — The defect-pinning rule.**
*Never encode known-defective behaviour as an expectation. Where the intended contract is known and currently violated, write the assertion to the intended contract and `[Ignore("DiVoid #NNNN")]` it.*

Without this rule the fresh-blood band test would assert `Elitism − 1` slots — permanently cementing #9054 and, worse, guaranteeing a red build the day someone fixes it. With it, the suite stays green, the defect backlog becomes executable, and *removing an `[Ignore]` is the acceptance criterion of the fix*. It is also the cheapest possible S4 mechanism: a fix that lands turns a skipped test green instead of turning a passing test red.

**Sharpened after PR #4's QA round (DiVoid #9352), which found the rule as originally stated constrains only the assertion.** Two escapes surfaced in the same review:

- `Evolve_FreshBloodBand_MarksExactlyElitismSlots` correctly pinned the intended contract and was correctly `[Ignore]`d against #9054 — but its fitness-label fixture was built only for the *defective* two-slot band. Applying the fix and removing the `[Ignore]` produced a `KeyNotFoundException`, not a pass: the acceptance criterion R5 promises was not actually usable.
- `RivalismTests` encoded filed defect #9047 (cumulative rival mutation) directly into its fixture data, with no `[Ignore]` at all, because the test's *subject* was rivalism in general, not #9047 specifically. Simulating the fix turned it red — the exact S4 outcome R5 exists to prevent, arriving through the fixture instead of the assertion.

Three added clauses close both gaps:

1. **The pin must pass under the fix, not merely fail without it.** Before adding the `[Ignore]`, apply the fix locally, run the test, confirm green, then revert. A pin verified only red-on-arrival is half-verified; state the two-sided verification in the PR body.
2. **Fixtures must be fix-tolerant.** Where a label map, scripted RNG or slot index encodes the defective path, widen it to cover both paths (a superset map, or a double that does not key on the value the fix moves) so the fix changes the verdict and nothing else.
3. **The trigger is "is this behaviour filed?", not "does this look wrong to me".** Before writing any expectation, search DiVoid for the mechanic under test. If a `bug`/`task` node describes it, R5 binds — regardless of whether the test's subject *is* that defect.

**RESOLVED 2026-08-24 — the `[Ignore]` pin and a tripwire test are complements, not alternatives.** Two PRs in this chain made apparently opposite calls on the same question. PR #2 rejected a tripwire in favour of an `[Ignore]`d intent pin for #9043, reasoning that a tripwire goes red on a *good* change (counter-goal S4) and cannot express intent — a pinned collapsed hash reads identically to a belief that the collapse is correct. PR #4 shipped **both** for #9047, and QA required the split. The contradiction is only apparent: PR #2's reasoning was never against tripwires as such, it was against a tripwire used *instead of* an intent pin.

- The **`[Ignore]`d intent pin** asserts the contract we want. It is the primary artefact: red-if-un-ignored today, **green on the fix**. Never optional.
- The **tripwire** documents current defective behaviour. It is green today, **red on the fix**, and exists so the change cannot land unnoticed.

A tripwire alone is what PR #2 rightly rejected. A tripwire *alongside* an intent pin has neither problem — the intent is stated next door, and the tripwire's own `[Description]` declares what it is.

A tripwire is acceptable only under four conditions: (1) an `[Ignore]`d intent pin exists alongside it, asserting the contract actually wanted; (2) its `[Description]` says explicitly that it is expected to go red on the fix, and points at the sibling — PR #4's wording, carried by `RivalismTests` until #9047 was closed in PR #13 and replaced there, is the reference: *"Documents CURRENT (defective) behaviour, not the contract… This test is expected to go red the day #9047 is fixed — see the sibling intended-contract pin above."*; (3) the defect task's acceptance criteria name both tests and both terminal actions — remove the `[Ignore]` from the pin, and either **delete** the tripwire or **invert** it to assert the new behaviour. Inversion is permitted only where it yields a test that can actually fail: at least one mutant of the fixed code must kill the inverted test, demonstrated rather than argued. An inversion that no mutant kills is coverage theatre and the tripwire must be deleted instead. Note that an inverted test may be strictly subsumed by the intent pin — killed by no mutant the pin does not also kill — and still be worth keeping for assertion quality or as the home for a filed follow-up; that is a judgement to state in the PR, not a fifth condition. See #9043 (closed by deletion) and #9047 (closed by inversion in PR #13, whose inverted test was strictly subsumed by the intent pin on arrival and earned its own kills when #9931 and #9936 were closed into it); (4) both directions are verified — simulate the fix and confirm the pin goes green *and* the tripwire goes red (this is CF2 above, generalised).

**What is still forbidden: a test that depends on a defect without declaring it.** That was PR #4's CF1 — the original rivalism test's name promised "evaluates exactly `Rivalism` candidates and keeps the best," both invariant under the fix, while its assertions silently depended on the cumulative chaining. It would have gone red on a good change with nothing anywhere saying why. **The harm was never the dependency. It was the dependency being undeclared.**

Reviewer check: *does any assertion's expected value trace to a filed defect?* For every `[Ignore]`d pin, apply the referenced fix and confirm green. For every test whose fixture encodes a mechanic with an open defect node, apply that fix and confirm the test survives. For every tripwire, confirm an intent pin exists beside it, its `[Description]` names the sibling and the expected direction, and the defect task's acceptance criteria name both.

---

**R6 — The independent-oracle rule** (added 2026-08-23, from QA #9084).
*Prefer an oracle the production path cannot influence. Where a literal is unavoidable, derive it symbolically — never capture it from a test run.*

R2 asks what pre-image the *fixture* pins. R6 asks the prior question: is the expected value independent of the code at all? An expected value obtained by running the code and pasting the output is still derived from the code — it wears the costume of an independent expectation while pinning whatever the implementation does, including any error in it. That is the unfalsifiable-expectation failure in its least visible form, because the artefact looks like a hand-written constant.

Good oracles, in preference order: a BCL function the code under test does not call (`Math.Sin`, `Math.Tanh`, true square roots); a hand-computed exact rational; a value derived from the *specification* rather than the implementation.

**The reviewer check is unusually mechanical for a `double` literal pinning `float` arithmetic: it is decidable from the trailing mantissa bits.** A `float` widened to `double` leaves 29 low zero bits; a symbolically-derived rational does not. Worked example from the PR that earned the rule — the `Sigmoid` test at input −10:

| | decimal | bits |
|---|---|---|
| test literal | `0.045454545454545456` | `0x3FA745D1745D1746` |
| exact `1.0/22.0` | `0.045454545454545456` | `0x3FA745D1745D1746` ✅ |
| shipped impl, widened from `float` | `0.04545453190803528` | `0x3FA745D1`**`00000000`** ❌ |
| closed form evaluated in `double` | `0.04545454545454547` | `…1748` ❌ |

The literal matches the exact rational bit-for-bit and matches **nothing else** — not the implementation, and not even the double-precision *evaluation* of the closed form, which lands 1–2 ULP off through accumulated rounding. The only path to that bit pattern is symbolic reduction to `1/22`.

Where the mantissa technique does not apply (non-float types, values with no clean rational form), fall back to asking the implementer how the number was obtained and requiring the derivation in the return.

**Interaction with the speed-over-accuracy philosophy (#9083):** R6 governs *where the expected value comes from*; #9083 governs *what property is worth asserting at all* on a deliberately-approximate routine — finite, correct sign, monotonic, order-preserving, plus a wide catastrophic bound, rather than a tight accuracy band. Both apply: when a band is genuinely warranted, its centre still has to be an independent oracle rather than a captured value.

Reviewer check: *was this expected value derived, or captured?* For a `float`-precision literal, check the trailing mantissa bits.

---

**These six rules go in the test project as a short `README.md`.** Not in agent memory, not in a DiVoid node only — in the directory where someone about to write a test will trip over them.

### 10.2 Determinism hazard inventory

Every source of non-determinism in a training run, and its disposition at `Threads == 1`. This table is the evidence behind assumption A3; the implementer should re-verify each row against the source rather than trusting it.

| # | Hazard | Status at `Threads == 1` | Disposition |
|---|---|---|---|
| H1 | `Train` constructs its RNG internally, clock-seeded | fatal — no reproducibility at all | **P1 + P2**: honour `setup.Rng` |
| H2 | `Evolve`'s re-score uses `Parallel.ForEach` **unconditionally** | `MaxDegreeOfParallelism = 1` makes it *effectively* serial, but source-order iteration is not contractual | **P3**: branch to a sequential loop |
| H3 | `Mutate` uses `Parallel.For` when `Threads > 1` | already branches; sequential at 1 | none |
| H4 | `Train`'s initial scoring uses `Parallel.ForEach` when `Threads > 1` | already branches; sequential at 1 | none |
| H5 | `GenePool.Next` mutates shared ancestry state (`originCount`, `Remove`) under a lock | order-dependent, but the order is deterministic at 1 thread | none. **This is the single reason parallel determinism is out of reach** — see §11.2 |
| H6 | `Guid.NewGuid()` for `AncestryId` | values vary run to run, but are used only as dictionary keys and equality filters, never for ordering or selection (A4) | none; documented. If the determinism test fails and everything else is accounted for, this is the suspect |
| H7 | `SamplesEvaluator` caches `indexedSamples` from the first chromosome and pools nets in a `ConcurrentStack` | pop/push order deterministic at 1 thread, but state **carries across runs** | harness contract: **fresh evaluator per run** |
| H8 | `Train` writes `setup.Mutation.Runs` (in/out parameter, #9029) | a second `Train` on the same setup object does not start from the configured value | harness contract: **fresh `EvolutionSetup` per run** |
| H9 | `float` non-associativity and libm differences across machines/runtimes (A2) | identical within one process | **never compare against a committed literal fitness** — §11.3 |
| H10 | `Rng`'s clock fallback when the seed is `0` (#9038 item 4) | a caller writing `new Rng(0)` gets a clock seed, not seed zero | not fixed here. The harness and tests use non-zero seeds. Documented in P1's XML doc as a caveat; it disappears when #9038 lands |

H7 and H8 are the two that would silently corrupt a benchmark rather than fail loudly. They are stated as harness contracts precisely because there is no compiler support for them.

### 10.3 Error handling, observability, performance

- **Error handling in tests:** a double that cannot satisfy a request throws with a message naming what it was asked for and what it holds (R3). Diagnostic value beats leniency in every case.
- **Observability of the benchmark:** the printed comparison table is the entire observability story. No logging framework, no telemetry, no persisted history beyond the single committed baseline. (Design Contracts §2 Form 1 — a history store with no named consumer is a data dump.)
- **Performance budget:** Lane 1 in the low seconds. Lane 2 in minutes, bounded by 3 problems × 8 seeds = 24 runs, parallelised across pairs. On a multi-core machine at `Threads = 1` per run, core utilisation is *better* than today's `Threads = 2` demo tests, not worse.
- **Concurrency:** every fixture stays `[Parallelizable]` per Code Contracts §13. Doubles are per-test instances; the only shared state is the read-only baseline file.
- **Security / auth / caching / idempotency:** not applicable — a library test suite with no I/O beyond two local files.

---

## 11. Quality Attributes & Trade-offs

Three decisions. Each names its rejected alternatives and why.

### 11.1 D1 — The determinism seam is an injected `IRng`, not a seed

**Decision:** `EvolutionSetup<T>` gains an `IRng Rng` property. The caller constructs the stream and hands it in.

**Rejected — `long? Seed` on `EvolutionSetup`:**

| | `IRng Rng` (chosen) | `long? Seed` (rejected) |
|---|---|---|
| Production surface | 1 property + 1 guard + 1 branch | 1 property + 1 guard + 1 branch **+ a change to `Rng`'s constructor**, because `Rng(0)` currently means "seed from the clock" — the one seed a reproducible test naturally reaches for is the one value that cannot be requested (#9038 item 4) |
| Streams per run | one — the same instance builds the population and drives training | two independent `Rng` instances constructed from the same seed produce **identical streams**, so population construction and training would draw the same sequence for different purposes. A subtle, real statistical hazard |
| "Did you also seed the `Population`?" | forced into view — you must hold the instance to pass it | easy to forget; the run looks seeded and is not |
| Scripted `IRng` in mechanics tests | supported, and it is what makes `GenePool`'s roulette assertable (§6.2) | impossible — a seed cannot script a draw |
| Coupling to #9038 | none | prerequisite |

The last row decided it. A seed-shaped seam makes this design depend on a defect fix sitting in the backlog; an `IRng`-shaped seam makes it depend on nothing.

**Rejected — a per-worker `IRng` factory:** it is the right shape *if* parallel determinism is the goal. It is not (§11.2), and building the factory without that goal is an abstraction with one implementation and no concrete second (Design Contracts §4).

**Also considered and declined — widening `Population`'s constructor from `Rng` to `IRng`.** It is a four-character, source-compatible change that would remove a genuine inconsistency (everything else in the library takes `IRng`). It is declined because **no test needs it**: mechanics tests use the `PopulationEntry<T>[]` constructor and need no generator at all, and the benchmark harness passes a concrete `Rng`. YAGNI. It stays free to do the day a consumer appears.

**Trade-off accepted:** callers write two lines instead of one to get a reproducible run. That is the entire cost.

### 11.2 D2 — Determinism holds at `Threads == 1` only, and the API refuses to pretend otherwise

**Decision:** single-threaded determinism is the contract. `setup.Rng` combined with `Threads > 1` throws.

**Why not parallel determinism.** Here is the cost, concretely. Parallel determinism needs **three** things, not one:

1. **Per-slot independent streams** in `Mutate`'s `Parallel.For` and in `Evolve`'s re-score, so a draw depends on the slot index rather than on which worker got there first. This requires the `IRng` contract to gain a notion of splitting — a new member on an interface with two implementations and call sites throughout both net families.
2. **Deterministic `GenePool` draw ordering.** Even with per-slot streams, `GenePool.Next` mutates shared ancestry state: `originCount` increments and the 5-draw retirement depend on the *order* draws arrive across threads. Per-slot streams do not fix this. Fixing it means redesigning `GenePool` — a component carrying open defect #9042 (lineage exhaustion), whose correct shape is genuinely unsettled.
3. **Deterministic net-pool assignment** in `SamplesEvaluator`'s `ConcurrentStack`, which is #9039 territory (stale state carrying between individuals).

Three subsystems, two of them holding open defects whose fixes will change their shape. And every one of those changes **alters training behaviour**, which moves the baseline the measurement layer is being built to hold steady.

**Why the cost buys nothing.** The apparent reason to want parallel determinism is benchmark throughput. It is not needed, because **the benchmark parallelises across seeds, not within a run**: 3 problems × 8 seeds = 24 independent single-threaded runs saturate a multi-core machine *and* each one is individually reproducible. The threading axis the benchmark needs is the one it already has for free.

Design Contracts §1 (YAGNI) and §2 (existing systems first) both point the same way. No consumer needs a reproducible parallel run today. Building it now means guessing the shape of three fixes that have not happened.

**Trade-off accepted, stated concretely:** a concurrency bug that manifests **only** at `Threads > 1` cannot be caught by a deterministic regression test. That is a real gap — `LockedRng` exists to fix exactly such a bug (commit `b95c11b`) and has no regression test (#9038). The mitigation is that concurrency regressions remain testable by *stress* rather than by *replay*: `SamplesEvaluatorTests` already does this (2000 parallel evaluations, then assert the sample cache survived), and that pattern extends to `Population` — run threaded, assert invariants that must hold regardless of ordering (no NaN, no lost entries, population size preserved). **Non-deterministic tests that assert order-independent invariants are legitimate; non-deterministic tests that assert *outcomes* are what this design exists to eliminate.**

**The seam does not foreclose the growth.** `EvolutionSetup.Rng` being an `IRng` rather than a `long` means a splitting generator can be introduced later at the same injection point, with the actual shape in hand. **Nothing about that growth is designed or built now** — no splitting member, no factory type, no per-worker vocabulary appears anywhere in this design. It is named here only so a future reader knows the door is not nailed shut.

### 11.3 D3 — The benchmark asserts invariants and *reports* quality

**Decision:** the benchmark lane asserts two things that can only become more true as the library improves, and reports everything else as a table for a human to read.

**The failure mode being designed against, stated plainly:** a benchmark that asserts "median fitness ≤ 0.31" goes red the day #9037's guard lands, the day #9041's penalty direction flips, the day #9043's structure hash starts distinguishing neuron configuration. Each of those is a *fix*. A suite that punishes fixes gets switched off, and then nothing is measured at all — a worse outcome than today.

**The two invariants:**

**I1 — every recorded fitness is finite and non-negative.**
A NaN or ±∞ final fitness means the pathology in #9037 occurred: `NaN < 0.0f` is false so the validity filter passes it, and `float.CompareTo` orders NaN below every real number, so it is permanently rank 0 and no fresh chromosome — however good — can ever outrank it. A negative final fitness means a disqualified entry won. Neither can be a legitimate consequence of any fix. **This invariant becomes more true, never less, as defects are repaired.**

**I2 — every run's final fitness is at most its own generation-0 best.**
Training must not end worse than it started. At `SampleCount = 0` — the default, and what the benchmark problems use — this holds *by construction of elitism*: the best entry is copied forward and re-scored on the identical full sample set, so the leader's fitness is non-increasing across generations, and the final full-set re-score is on the same scale. A violation means elitism is not preserving the leader. That is a genuine, high-value guard on the mechanism the whole library rests on.

**Both invariants are self-relative.** I2 compares a run against *itself*, not against a committed number. No machine-dependence (A2/H9), no rot, no re-recording. That property is what makes strictness affordable here.

**Everything quantitative is reported, not asserted:**

- **Per-seed paired comparison.** Seeds are fixed, so baseline and current can be compared *on the same seed*, which removes most of the variance a median hides: *"improved on 5 of 8 seeds, regressed on 2, unchanged on 1"* is a far stronger signal than a median delta. **Caveat that must be printed in the report itself:** pairing is only meaningful while the RNG *implementation* is unchanged. When #9038's recurrence fix lands, the same seed produces a different stream and per-seed pairing becomes meaningless — only the distribution comparison survives. The report must say so rather than letting a reader draw a false conclusion.
- **Distribution per problem:** median / min / max final fitness, count of seeds reaching `TargetFitness`, median generations.
- **No verdict word.** No BETTER/WORSE badge. A verdict computed from 8 stochastic runs invites confidence the data does not support; the numbers and the human are enough.

**The baseline lifecycle — how a moved baseline reads as information:**

```
   a change lands that alters training behaviour (#9037 / #9040 / #9041 / #9043 / #9038 ...)
        |
        +- developer runs the benchmark     -> the table shows what moved, and by how much
        |
        +- developer runs RecordBaseline    -> baseline.json is rewritten in the source tree
        |
        +- the PR carries the new baseline as part of its diff, with `note` saying which
           change moved it and `commit` pinning when
                 |
                 +- the reviewer reads a DIFF of numbers, with a stated cause,
                    instead of triaging a red build
```

**Rejected — asserting a quality floor (e.g. "at least 6 of 8 seeds must solve"):** either loose enough that it cannot fail (the sin this whole document exists to eliminate) or tight enough to be flaky on a stochastic search. Both outcomes are worse than reporting.

**Rejected — a persisted history of every benchmark run:** Design Contracts §2 Form 1. No named consumer, no decision it enables within four weeks. One committed baseline plus git history *is* the history.

**Rejected — a separate `Pooshit.Ai.Benchmarks` console project:** Design Contracts §2 Form 2, a parallel layer. NUnit is already referenced, `[Explicit]` already provides lane separation, `--filter` already provides selection. A new csproj buys nothing and costs a build target, a launch story and a second place to keep test doubles.

### 11.4 Quality attributes summary

| Attribute | How the design serves it |
|---|---|
| **Maintainability** | Two lanes, one project, no new csproj, no new framework. The doubles are extensions of files that already exist. |
| **Diagnosability** | The determinism test compares *trajectories*, so a failure names the generation where two runs diverged. The stub evaluator's call log names which chromosome was scored when. |
| **Speed** | The default lane contains no long training. The one end-to-end test is 20 individuals × 30 generations. Demos become `[Explicit]`. |
| **Survivability across fixes** | I1/I2 are monotone in quality; the baseline is a record with provenance; R5 turns pending fixes into `[Ignore]`d tests that go green when the fix lands. |
| **Resistance to unfailable assertions** | Five named rules (§10.1), each converting an invisible property into an artefact whose absence a reviewer can see. |
| **Extensibility** | Adding a benchmark problem is one entry plus a re-record. Adding a mechanics test needs no new infrastructure. Parallel determinism, if it is ever wanted, attaches at the same `IRng` injection point — with nothing built for it now. |

---

## 12. Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R-1 | The determinism test **fails on arrival** — a source of non-determinism remains that §10.2 did not find. | Medium | High: Phase 2 blocks. | This is the test doing its job. The trajectory comparison names the first divergent generation. Work the hazard table (H5, H6, H7 are the candidates). **Do not weaken the test to pass.** If the cause is a defect, file it and `[Ignore]` per R5. |
| R-2 | I2 (final ≤ generation-0 best) **fails on arrival** because #9039's stale net state makes an elite re-score differently. | Medium | Medium | The benchmark lane is `[Explicit]`; a red benchmark breaks no build. A first-run violation is a genuine finding about elitism — file it, keep the assertion. This is precisely the value the lane is being built for. |
| R-3 | The behavioural serialization round-trip fails because the round trip is lossy on input-neuron `Activation`/`Aggregate` (which the existing tests blank before comparing). | Medium | Low | File as a defect; `[Ignore]` per R5; keep the structural test meanwhile. |
| R-4 | Benchmarks are too slow and stop being run. | Low | High | 3 problems × 8 seeds, `Runs` capped per problem, parallel across pairs. If it exceeds a few minutes, reduce `Runs` before reducing seeds — seed count is what makes the distribution meaningful. |
| R-5 | The baseline drifts stale because nobody re-records after a behaviour-changing merge. | Medium | Medium | The report header prints the baseline's `recordedAt`, `commit` and `note` alongside the current numbers, so staleness is visible every time the lane is run. |
| R-6 | The six rules in §10.1 decay into folklore. | Medium | High — this is the failure that produced the current suite. | They live as a `README.md` in the test project, and QA review of each test PR checks them by name. A rule nobody can point at is not a rule. |
| R-7 | The `Threads > 1` guard breaks an existing caller. | Very low | Low | The guard fires only when `setup.Rng` is set, and that property does not exist yet. Every current caller passes `null` by omission. |
| R-8 | Mechanics tests over-fit to private implementation detail and become churn on every refactor. | Medium | Medium | Assert only on observables: `Entries` ordering and reference identity, and double call logs. Never reach into internals via reflection or `InternalsVisibleTo`. |

---

## 13. Migration / Rollout Strategy

Four phases, four PRs, in dependency order. One independently-meaningful unit per PR. Each phase ends with a suite that is green and faster than it was.

**Phase 1 — Pure units, hardened doubles, lane hygiene.** *(test project only; no production change)*
Harden `FakeNet` and extend `SequenceRng` per R3; add the rules `README.md`; mark `CalculatorTests` `[Explicit, Category("Demo")]`; write the Lane 1a tests (§9.1) under R1/R2/R4.
Exit: the default lane runs in seconds and contains real assertions on `NMath`, `AMath`, `EnumerableExtensions` and the mutation ladders — the surfaces where the confirmed defects live.

**Phase 2 — The determinism seam + the determinism pair.** *(production change P1/P2/P3 + tests)*
P1, P2, P3 exactly as specified in §8.1 and nothing else; the guard test; the determinism/non-determinism pair (§6.1).
Exit: a training run is reproducible on request, and a test proves it. **This is the unlock the symptom mapping (#9060) calls the highest-leverage change in the repo** — it converts "unsure whether it's a bug or variance" from unanswerable to measurable.

**Phase 3 — Genetics mechanics.** *(test project only)*
`StubFitnessEvaluator<T>`, the three reproduction-strategy doubles, `FakeChromosome` parametrised; the Lane 1b tests (§9.2), with R5 `[Ignore]`s where a filed defect blocks the intended contract.
Exit: elitism, dedup, ordering, selection, rivalism, fresh blood and escalation are all covered without a single net, sample or float tolerance.

**Phase 4 — The measurement lane.** *(test project + one data file; depends on Phase 2)*
The three `BenchmarkProblem`s, the fixed seed set, `BenchmarkHarness`, `BenchmarkComparison` with I1/I2 and the report table, `RecordBaseline`, and the first recorded `baseline.json` with `note: "first recording"`.
Exit: S2 is met — one command produces a per-problem distribution and a comparison against a recorded state.

**Sequencing note for the orchestrator:** Phase 3 depends on nothing in Phase 2 (it uses the `PopulationEntry<T>[]` constructor and a stub evaluator) except the scripted-`IRng` cases, and can run concurrently with Phase 2 if two implementers are available. Phase 4 must follow Phase 2.

**No deprecation period, no feature flag, no compatibility shim.** `setup.Rng` defaults to `null`, which is today's behaviour exactly; there is nothing to transition.

---

## 14. Design Contracts #1136 §5 — Pre-Design Checklist audit

**KISS / DRY / YAGNI**

| Item | ✓ | Where |
|---|---|---|
| No new type mirroring an existing type's value-space (Code Contracts §5.4) | ✓ | §5.2 — the doubles **extend** `FakeChromosome` / `FakeNet` / `SequenceRng`; a parallel `StubChromosome` is explicitly forbidden. `StubFitnessEvaluator<T>` has no counterpart (`SamplesEvaluator` is the only shipped `IFitnessEvaluator`, and it is a *net-driven* evaluator, not a stub). |
| No new abstraction with one implementation and no concrete second | ✓ | §11.1 — a per-worker `IRng` factory is rejected on exactly this ground. No new interfaces are introduced at all; the seam reuses `IRng`. |
| No element justified by "we might need X later" | ✓ | §11.2 — parallel determinism is **rejected**, not deferred behind a hook, and nothing is built for it. §7 — no baseline format-version field. §11.3 — no run-history store. |
| No deprecation period / feature flag / compatibility shim | ✓ | §13 — `setup.Rng == null` *is* today's behaviour; there is nothing to transition. |
| DRY math quoted for any "inline at N sites" decision | ✓ | No inline-at-N-sites decision exists in this design. The one DRY-adjacent call is P3, which **removes** an inconsistency: `Train` and `Mutate` already branch on `Threads`; `Evolve`'s re-score is the single site that does not — 1 site, alignment not duplication. |

**Existing systems first**

| Item | ✓ | Where |
|---|---|---|
| Audited whether an existing surface already covers the concern | ✓ | §5.2 (existing doubles extended, not replaced), §11.3 (NUnit + `[Explicit]` + `--filter` already provide lane separation), §8.1 (the seam reuses `IRng` and `EvolutionSetup` rather than introducing a configuration object). |
| New layer's concrete reason to exist is named | ✓ | §11.3 — a separate benchmarks project is **rejected** as a §2 Form 2 parallel layer. The only genuinely new components are `StubFitnessEvaluator<T>` (no existing type can return a scripted fitness) and the benchmark harness (no existing type runs a problem × seed matrix). |
| New persisted data enables a concrete decision within 4 weeks | ✓ | §7 — `baseline.json`'s consumer is the developer comparing before/after a defect fix, and those fixes are queued now (#9036 merged 2026-08-23; #9037/#9038 next). `note` and `commit` have the named consumer "the human reading the PR diff". Wall-clock time is **not** recorded, for lack of a comparable frame. |
| Fields justified by "an existing reader projects it" have the consumer chain recursed | ✓ | Not applicable — no DTO/projection chain exists here. Every recorded field (`FinalFitness`, `Generations`, `recordedAt`, `commit`, `note`) is consumed by the report table in §6.3. |

**Configurability**

| Item | ✓ | Where |
|---|---|---|
| Every new knob has a named operator or environment difference | ✓ | **No configuration knobs are introduced.** The seed set, problem set, population sizes and `Runs` caps are `const`/`static readonly` in the harness (§5.3, §7). No benchmark config file, no environment variable, no test-run parameter. |
| "Telemetry-then-tune" knobs paired with a filed task | ✓ | Not applicable — no such knob exists. |
| Magic numbers that need not vary stay `const`, clearly named | ✓ | §5.3 / §7 / §16 — seed count (8), per-problem `Runs` caps and population sizes are named constants in the harness. `EvolutionSetup.Rng` is a value the caller supplies, not a configured one. |

**Less is better**

| Item | ✓ | Where |
|---|---|---|
| Every element passed can-it-be-deleted / merged / inlined | ✓ | §11.3 — a separate benchmarks project, a run-history store, a quality-floor assertion and a BETTER/WORSE verdict word were each considered and **deleted**. §11.1 — widening `Population`'s constructor to `IRng` considered and **declined** for want of a consumer. §7 — the format-version field deleted. An intermediate `baseline.candidate.json` was considered and deleted in favour of `RecordBaseline` writing the source file directly (§16 step 19). |
| Trade-offs named explicitly where the complex option won | ✓ | §11.1 (two lines at the call site instead of one), §11.2 (concurrency bugs at `Threads > 1` are not replay-testable — stress-testing named as the mitigation), §11.3 (no quality-regression alarm — the paired table is the substitute). |
| Radical-clean shape chosen when the existing surface has no consumer | ✓ | §11.2 — parallel determinism rejected outright rather than half-built behind a splitting hook. §11.3 — one committed baseline rather than a history plus a baseline. |
| Reader inventories cover AST **and** string-literal references | ✓ | §8.1 — the production change adds a property and branches two call sites, both in `Population.cs`. There are no string-literal references to `EvolutionSetup` members anywhere in the repo (no LowCode predicates, no field-name arrays, no mapper indexers). |
| Carrier-swap tables enumerate every affected DTO | ✓ | Not applicable — no property is removed or relocated. `EvolutionSetup` gains one property; nothing is moved. |

**Data deliverables** — not applicable. This design produces no SQL, no migration, no backfill.

**Document discipline**

| Item | ✓ | Where |
|---|---|---|
| Cites Code Contracts (#114) and Design Contracts (#1136) as load-bearing | ✓ | Header; §10.1 and §16 (Code Contracts §13 test conventions); §14 (this audit). |
| Reader / scope inventories explicit | ✓ | §2 (scope), §8.1 (the complete production change — three edits, exhaustive), §9 (the complete test inventory), §10.2 (the complete determinism hazard inventory). |
| Out-of-scope items listed explicitly, not merely absent | ✓ | §2 "Explicitly out of scope" — six named categories covering every referenced defect number. |
| No multi-paragraph "rationale for keeping X" for things that obviously stay | ✓ | `CalculatorTests` gets one sentence (§9.4). `Rng` / `LockedRng` / `IRng` get one table row (§5.1) saying "unchanged". |
| Superseded predecessor designs banner-marked | ✓ | Not applicable — `docs/architecture/` did not exist before this document; this is the first design in the repo. |

---

## 15. Open Questions

**For the orchestrator — none blocking.** The design is complete enough to implement as written. Nothing in the brief forced a KISS/DRY/YAGNI violation, and the determinism problem is solvable within the declared scope (three edits, §8.1), so neither architect-side bounce condition (Design Contracts §7) applies.

**Q1 — ANSWERED by Toni, 2026-08-23: the design's three problems stand as specified, no design change required.**
`BinOp.MultiplyMinus`, `FeedForward.MultiplyMinus`, `FeedForward.Gender`, all lifted from existing `CalculatorTests` so that **no new training data is authored**:

| Problem | Family | Shape | Why it is in the set |
|---|---|---|---|
| `BinOp.MultiplyMinus` | `DynamicBOConfiguration` / `DynamicBONet` | 3 inputs → 1 output, 21 samples, exact solution exists (`x*y − z`) | measures exact-formula discovery — can it *solve* it |
| `FeedForward.MultiplyMinus` | `DynamicFFConfiguration` / `DynamicFFNet` | the same 21 samples | the same problem in the other family — isolates "which family is better at this" from "which problem is harder" |
| `FeedForward.Gender` | `DynamicFFConfiguration` / `DynamicFFNet` | 20 inputs → 3 outputs, 75 samples, no exact solution, `TargetFitness = 0.01` | measures convergence quality on a realistic noisy classification |

Toni chose these over adding a known-bad problem and over naming a different set. The paired `MultiplyMinus` entries — the same problem in both net families — are the part worth protecting in any future revision, since they are what separates *"which family is better at this"* from *"which problem is harder."* **PR 4 implements this exactly as written.**

Worth noting for whoever revisits this: the *rejected* alternative (a hard/known-bad problem that would let the benchmark show a defect fix *working* rather than only show nothing regressed) remains a reasonable later addition. Once #9037 / #9040 / #9043 land, a problem known to stall today would turn the benchmark from a regression detector into a demonstration that the fixes helped — and adding one costs a single entry plus a re-record, exactly as this section always said.

**Q2 — ANSWERED by Toni, 2026-08-23: eight seeds, as designed.** Offered against 16–20 for tighter distributions; not taken. **PR 4 implements this exactly as written.**

**Q3 — Deferred to #9045, not decided here: the dead data fixtures.**
`testmodel.json` and `xyzmodel.json` use a `layers`/`layerSize` format **no type in the current library reads**; `excellence_broker_samples.json` is a prepared benchmark never wired up. All three are still `CopyToOutputDirectory`. They belong to #9045's inventory and touching them here would widen this PR chain for no benefit. Flagged so it is not lost.

**Q4 — For the implementer, resolvable during Phase 2 without coming back:** if the determinism test fails on arrival, work §10.2's hazard table (H5, H6, H7 are the candidates, in that order) and file whatever it finds as a defect. Do **not** weaken the assertion to make it pass — a determinism test that has been relaxed to pass is the exact artefact this whole document exists to prevent.

---

## 16. Implementation Guidance for the Next Agent

Ordered work breakdown at the architectural-unit level. The numbered groups are PR boundaries.

### PR 1 — Regression lane foundations *(test project only)*

1. Harden `FakeNet` per R3: record `SetInputValues`, indexer writes and `Update` arguments; throw from the string indexer. Keep its deliberate constant-zero oracle behaviour — the existing `SamplesEvaluatorTests` depend on it and are correct.
2. Extend `SequenceRng` to script `NextFloat`, `NextDouble`, `NextLong` and `NextInt()` with the same record-validate-throw discipline the existing `NextInt(max)` already has. That method is the template; copy its shape.
3. Add `Pooshit.Ai.Tests/README.md` carrying R1-R6 from §10.1 with their rationale.
4. Mark `CalculatorTests` `[Explicit, Category("Demo")]` at fixture level.
5. Write the Lane 1a inventory (§9.1). Drive every enum-driven surface from `Enum.GetValues` (R4). Use the probe **vector** for activations, not a single probe.
6. Verify the default lane runs in seconds.

### PR 2 — The determinism seam *(production + tests)*

7. Implement P1, P2, P3 from §8.1 **exactly as scoped, and nothing adjacent.** Do not fix `Rng`, do not deduplicate `LockedRng`, do not widen `Population`'s constructor. Those are #9038 and a declined item respectively.
8. Write the guard test: `Rng` set with `Threads > 1` throws; either alone does not.
9. Write the determinism pair per §6.1 — trajectory via `AfterRun`, final fitness vector, serialized winner; plus the mandatory different-seed sibling.
10. Keep it small: 20 individuals, 30 generations, a handful of samples.

### PR 3 — Genetics mechanics *(test project only)*

11. Build `StubFitnessEvaluator<T>` per §8.2. This is the keystone — build it first and the rest follows.
12. Parametrise `FakeChromosome`'s `StructureHash()` and `FitnessModifier`. **`FitnessModifier` must not default to `0.0f`** — it is used as a divisor in `Evolve` and the current value makes every breeding weight infinite, which is why the type is unusable in `Population` tests today.
13. Add `MutatingFakeChromosome`, `CrossingFakeChromosome`, `AmbidextrousFakeChromosome`, each recording its reproduction calls.
14. Write the Lane 1b inventory (§9.2), driving through `Train` with `Runs = 1` and observing `Entries` ordering, reference identity, and the doubles' call logs. Never reflect into privates; never add `InternalsVisibleTo`.
15. Apply R5 wherever the intended contract is currently violated — at minimum the fresh-blood band (#9054). `[Ignore]` with the DiVoid node id in the message.

### Test-double surface lifecycle — the unconsumed-double condition

PR 1's hardened `SequenceRng` (`NextFloat`/`NextDouble`/`NextLong`/`NextInt()`) and `FakeNet`'s recording surface were kept over a YAGNI objection, on the grounds that their consumers — PR 2's determinism pair, PR 3's mechanics tests — were named, designed and immediately next. **If PR 2 and PR 3 land and any of that surface is still unconsumed, it gets deleted then**, so "we will wire it next phase" cannot run indefinitely (the Uberkarl precedent, #8237).

**Corrected 2026-08-24, after applying the condition literally caused an R3 regression in PR 3.** The implementer deleted `FakeNet`'s `InputValues`/`IndexWrites`/`Updates` recording lists, correctly observing that no assertion read them — which reverted `SetInputValues` and `Update` to silent discards, the exact shape R3 names as its counter-example. Both members sit on `SamplesEvaluator`'s live call path and **cannot throw**, so recording was the only R3-compliant option available; deleting it selected the one behaviour R3 forbids. The rule was written to prevent speculative surface accumulating (and PR 3's deletion of `SequenceRng.NextLong()`/parameterless `NextInt()` under it was correct and stands — neither is called by any production path, ever) — but applied to something that was never speculative.

**The corrected condition:** surface kept for a *named future consumer* gets deleted when that consumer does not materialise. Surface that exists to *satisfy a contract* is **not** "unconsumed" merely because no assertion reads it yet. Before deleting any part of a test double, ask why it is there:

- **It anticipates a consumer** (a scripting method for a test not yet written) → the condition applies. Delete it.
- **It is what makes the double contract-compliant** (a recorder standing in for a throw the live path forbids; a validator; anything R3 requires) → the condition does **not** apply. Its consumer is the contract, not a test.

**The tell: if removing it changes the double's *behaviour* rather than only its *surface*, it was never unconsumed.** A recorder that nothing reads still converts a silent discard into an observable one — that observability is the deliverable; a future test needing it is a bonus, not the justification.

### PR 4 — The measurement lane *(test project + one data file)*

16. Build the three `BenchmarkProblem`s from §15 Q1, each constructing a **fresh** `EvolutionSetup`, `SamplesEvaluator` and `Population` per run (§10.2 H7, H8 — not optional, and there is no compiler support for it).
17. Build `BenchmarkHarness`: every pair at `Threads = 1`, parallel across pairs, producing `(ProblemName, Seed) → (FinalFitness, Generations)`.
18. Build `BenchmarkComparison` `[Explicit, Category("Benchmark")]`: assert I1 and I2; print the per-seed paired table, the per-problem distribution summary, the baseline's `recordedAt`/`commit`/`note` header, and the pairing caveat from §11.3 whenever an RNG-implementation change makes pairing void.
19. Build `RecordBaseline` `[Explicit]`: locate the source file by walking up from the test binary's directory until the `.csproj` is found, and overwrite `Benchmarks/baseline.json`. The walk-up is a handful of lines and removes the manual copy step that would otherwise kill adoption of the one workflow this design most needs to be frictionless.
20. Run it, record the first baseline with `note: "first recording"`, commit it.
21. If I2 fails on this first run, **that is the benchmark finding a real defect on its first outing** — file it against elitism / #9039 and keep the assertion. The lane is `[Explicit]`; nothing is broken by its being red.

### Standing constraints for every PR

- `[TestFixture, Parallelizable]` / `[Test, Parallelizable]`; `Assert.That(x, Is.EqualTo(...))`, never `Assert.AreEqual`; explicit types, never `var`; test names `MethodName_Condition_ExpectedResult` (Code Contracts §13).
- Every new test satisfies R1-R6 (§10.1), and the PR description states which rule each non-obvious assertion relies on. That sentence is what makes the rules reviewable rather than aspirational.
- No production change outside the three edits in §8.1. If implementing reveals that a fourth is genuinely required, **bounce to the orchestrator** rather than widening — the boundary between this design and the defect backlog is deliberately narrow, and widening it is how a test PR turns into a training-behaviour PR nobody can review.
