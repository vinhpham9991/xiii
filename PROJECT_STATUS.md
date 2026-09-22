# FRANKEN XIII — Project Status & Source-of-Truth Registry

**Status date:** 2026-09-22

**Repository:** `D:\Work\UNity\git\xiii\xiii`

**Baseline revision:** `c85b9d1` (`main`); Ego Reborn bug fixes, Boss HP Gate 60/40/20, and ID refactors implemented.

**Unity version:** `6000.5.8f1`

This file is the repository entry point for deciding what is design canon, what is currently implemented, and what has actually been verified. It does not replace the GDD.

## 1. Authority and precedence

When two sources disagree, use this order:

1. **Latest explicit Director decision marked LOCK.** A newer explicit Director decision overrides every document below it.
2. **Funding Demo contract.** For the May 2027 demo, use `12_FRANKEN_XIII_Funding_Demo_May2027_STANDARD_v1.10.1.md` from the GDD Standard Pack. The filename says `v1.10.1`, but its internal document code is `v1.10.5`; treat the internal code as the content version.
3. **Latest scoped update.** `FRANKEN_XIII_SESSION_SUMMARY_2026-09-10.md` / changelog `v1.10.6` overrides the Funding Demo contract only for the narrative changes it explicitly locks. Its historical implementation and test claims are not verification evidence.
4. **Battle terminology baseline.** `02_FRANKEN_XIII_Combat_System_STANDARD_v1.10.0.docx` and `FRANKEN_XIII_BATTLE_FULL_CONTEXT_2026-09-02_v1.10.0.md` define shared terminology and full-game architecture unless the Funding Demo contract explicitly overrides them.
5. **Current repository implementation.** Source, scenes, assets, packages, and ProjectSettings describe what exists now. They do not silently redefine a LOCK design rule.
6. **Fresh verification evidence.** A claim such as PASS, playable, or build-ready requires a current reproducible compile, build, automated test, or recorded playtest against the current revision.
7. **Historical handoffs and logs.** Older summaries, logs, backups, and superseded documents are context only.

The GDD Standard Pack currently lives outside this repository at:

`D:\Work\UNity\Project\FRANKEN_XIII_GDD_STANDARD_PACK_2026-09-02_v1.10.0`

Do not use the directory name alone as the content version. Several files received later internal revisions without being renamed.

## 2. Director decision 2026-09-20 — per-actor Beat model — LOCK

This decision supersedes the Funding Demo rule for six shared Action Nodes:

- Every combatant owns a fixed number of Beat slots per round instead of drawing from one shared action queue.
- Each playable Character has exactly **2 Beats** in the Demo.
- Each regular Enemy has exactly **1 Beat**; a Boss has exactly **3 Beats**.
- Resolution uses global Beat barriers: every scheduled action in Beat 1 resolves or cancels before Beat 2 begins, and Beat 2 completes before Beat 3.
- Actions from different actors inside the same Beat belong to the same rhythmic step. The exact effect-resolution policy inside one Beat is still **OPEN**; the current prototype launches those actions concurrently with small presentation offsets, so it must not be described as deterministic priority.
- Deleting an action removes only the latest action belonging to the currently selected actor. It does not remove another actor's action or clear that actor's complete plan.
- Player UI is organized per actor, with that actor's Beat slots shown on the same row.

The resulting Demo maximum is still six player actions when all three Characters fill both Beats, but it is a **3 Characters × 2 Beats allocation**, not a flexible six-node shared budget.

## 3. Funding Demo rules still relevant to this repository

- The Demo roster is XIII, Lê Mặc, and An.
- The older contract specifies six shared Action Nodes. This point is **SUPERSEDED** by the Director decision above; its Soul, Daze, Special, boss, and Playable Knowledge rules remain applicable unless separately overridden.
- Soul capacity is **3 Red Soul + up to 6 Blue Soul**. Costs consume Red first, then Blue.
- Red Soul refills at the start of a Player Phase. Blue Soul persists across rounds.
- Soul earned during Execute becomes available in the next Player Phase; it cannot rewrite the executing plan.
- Blue Soul rewards are authored as: +2 when Limit is broken into Daze, +1 on Crit, and +1 on Weakpoint when the hit is not already a Crit.
- There is no Guard command. Defense is proactive through Limit damage and Daze interruption.
- An's `Nhập Hồn` and Mặc's `Toàn Thức` are instant Special commands: Soul cost, 0 Action Node, two-round cooldown.
- XIII's `Bản Ngã Tái Sinh` is a scripted 0-Soul climax command unlocked at the 20% Boss HP threshold; the contract also requires an immediate 3 Red + 6 Blue refill, impact freeze, cut-in, finisher, and Phase 3 DPS-race handoff.
- Special presentation is part of the contract: `Nhập Hồn` requires a vignette/noise/audio treatment, and `Toàn Thức` requires an enemy reticle rather than text-only status.
- The boss encounter is a multi-entity contract: Bách Mệnh Quan plus two Hộc Tử Thi, phase transitions, 600 feedback damage, damage reduction while drawers protect the coffin, and top-down stun when the coffin is Dazed.
- Playable Knowledge must change combat or narrative outcomes; it is not optional flavor-only lore.

## 4. Current implementation status

Status terms:

- **VERIFIED:** reproduced on the audited revision.
- **IMPLEMENTED, UNVERIFIED:** source exists, but no fresh automated/runtime acceptance test proves the complete behavior.
- **PARTIAL:** a recognizable subset exists but differs from the locked contract.
- **MISSING:** no current implementation evidence was found.

| Area | Status | Current evidence / gap |
|---|---|---|
| Unity compile | VERIFIED ON CURRENT CHECKPOINT | Unity `6000.5.8f1` imported and compiled the uncommitted Task 3 working tree on 2026-09-21 with 0 C# errors as part of the fresh EditMode test run and Windows player build. |
| Windows player build | VERIFIED ON CURRENT CHECKPOINT | Fresh `StandaloneWindows64` build completed successfully on 2026-09-21. Artifact: `C:\Users\idola\AppData\Local\Temp\FrankenXIII-Task3-InstantSpecials-Build\FrankenXIII.exe`; log: `C:\Users\idola\AppData\Local\Temp\FrankenXIII-Task3-InstantSpecials-Build\build.log`. This verifies buildability, not PlayMode behavior. |
| Startup scene | VERIFIED | `Assets/_Project/Scenes/BattlePlaceholder.unity` is the only enabled build scene. |
| Plan -> Execute loop | IMPLEMENTED, UNVERIFIED | Player actions are grouped by Beat. All actions in the current Beat resolve/cancel before the next Beat begins, then enemy turn runs. Intra-Beat effect ordering remains OPEN; full PlayMode acceptance is pending. |
| Per-actor Beat budgets | IMPLEMENTED, UNVERIFIED | Each Character has 2 Beats, each regular Enemy has 1, and Boss has 3. Six EditMode rule cases verify these budgets, capacity rejection, and latest-selected-actor Beat lookup; runtime interaction acceptance is pending. |
| Dual Soul 3 Red + 6 Blue | IMPLEMENTED, UNVERIFIED | Red-first reservation, exact Red/Blue refund provenance, Blue cap, and immediate reward state are implemented and covered by five EditMode domain tests. PlayMode HUD/interaction acceptance is pending. |
| Seven-layer damage pipeline | PARTIAL | Core formula exists and `Nhập Hồn` now applies x2 damage/Break to An's next resolved offensive skill. Several effects and actor identities remain string-driven, and full numeric pipeline tests are still missing. |
| Break / Daze | IMPLEMENTED, UNVERIFIED | Daze rewards only on the transition, awarding +2; Crit awards +1 and Weakpoint awards +1 only when the hit is not Crit. Seven reward-rule test cases pass; runtime acceptance is pending. |
| Trio active skills | PARTIAL | Three runtime-created skills per character exist. Definitions are hard-coded by character-name checks rather than authored data assets. |
| Instant Specials | PARTIAL | `Nhập Hồn` and `Toàn Thức` execute in Plan Phase, cost 1 Soul Red-first, consume 0 Beats, and use a two-round cooldown. `Nhập Hồn` persists until An resolves a skill and cannot be stacked while charged. `Toàn Thức` charges Soul/cooldown only after a live enemy without an existing Weakpoint is confirmed. XIII's command is 0 Soul/0 Beat; the inclusive `<=20%` unlock, 3 Red + 6 Blue refill, and `egoRebornActivated` state flag exist. Runtime routing still relies on actor/boss display-name substrings, and full PlayMode acceptance is pending. |
| Special presentation / scripted climax | PARTIAL | Runtime battle messages, Special button labels/states, and a text `[WEAKPOINT]` marker exist. The authored Toàn Thức reticle, Nhập Hồn vignette/noise/audio, pre-charged Nhập Hồn before the Boss, impact freeze, XIII cut-in/finisher, Phase 3 transition, and DPS-race effect are missing. A lethal hit can currently skip the `<=20%` unlock because there is no Boss HP gate/phase transition. Keyboard confirmation also bypasses `Button.interactable`; most handlers guard invalid casts, but an already-active XIII Special can still be invoked again. |
| Boss multi-entity phases | MISSING | The scene has one boss and two support enemies, but no phase contract, 600 feedback damage, drawer protection, or top-down stun implementation. |
| Combat consumable contract | PARTIAL | Two demo items are created at runtime; the four locked items and per-item-type round restriction are not implemented. |
| Narrative / exploration / Playable Knowledge | MISSING | No five-area demo flow, dialogue, investigation, puzzle, world-state, or story-to-combat integration. |
| Automated tests | VERIFIED FOR CURRENT EDITMODE DOMAIN SCOPE | Unity Test Runner on `6000.5.8f1` passed 32/32 EditMode cases, fail 0, skip 0, inconclusive 0, against the uncommitted working tree based on `c85b9d1` on 2026-09-21. Artifact: `C:\Users\idola\AppData\Local\Temp\FrankenXIII-Task3-InstantSpecials-Validation\EditMode-results-retry.xml`. This covers Beat, Soul, reward, and Special rules; it is not PlayMode integration acceptance. |
| Visual/runtime acceptance | UNVERIFIED | No recorded full playthrough, Special interaction smoke test, or multi-aspect UI acceptance run is attached to this revision. The text Weakpoint marker must not be reported as the authored reticle VFX. |

## 5. Verified repository facts

- Git branch `main` is based on `c85b9d1`, synchronized with `origin/main`.
- A fresh 2026-09-21 asset metadata scan found 0 missing `.meta`, 0 orphan `.meta`, and 0 duplicate GUID groups across 357 GUID-bearing metadata files.
- The 2026-09-21 player build completed with 0 C# errors and 29 unique compiler/analyzer warnings. The main categories are obsolete Unity object-search APIs, obsolete sprite/editor APIs, and unsupported/missing serialization metadata.
- The current Windows build directory is approximately 234.4 MiB. It is an external temporary artifact, not a versioned release package.
- The Windows artifact includes AI Inference/Sentis runtime resources that may be removable after a package-usage audit.
- Unity-generated mutations to `ProjectSettings/ProjectSettings.asset` and `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset` were excluded from the Task 3 checkpoint; no intentional package, ProjectSettings, or URP settings change is part of this work.

## 6. Task 3 traceability

| Concern | Runtime source | Rule/test evidence |
|---|---|---|
| Special identity, state, cooldown, targeting, Soul charge, Nhập Hồn consumption, XIII unlock/refill | [`BattleManager.cs`](Assets/_Project/Scripts/Combat/BattleManager.cs) | [`SpecialCommandRules.cs`](Assets/_Project/Scripts/Combat/Domain/SpecialCommandRules.cs); [`SpecialCommandRulesTests.cs`](Assets/_Project/Tests/EditMode/SpecialCommandRulesTests.cs) |
| Special menu labels/interactable state, battle messages, text Weakpoint status | [`BattleUIManager.cs`](Assets/_Project/Scripts/Combat/BattleUIManager.cs) | Requires PlayMode/UI acceptance; no integration test currently covers it |
| Toàn Thức click-target filtering | [`CharacterInteraction.cs`](Assets/_Project/Scripts/Interaction/CharacterInteraction.cs) | Requires PlayMode mouse/keyboard acceptance |
| Beat/Soul/reward rules | [`Combat/Domain`](Assets/_Project/Scripts/Combat/Domain) | 6 Beat + 5 Soul + 7 reward EditMode cases |
| Instant Special pure rules | [`SpecialCommandRules.cs`](Assets/_Project/Scripts/Combat/Domain/SpecialCommandRules.cs) | 14 EditMode cases |
| Damage Pipeline rules | [`DamageCalculatorRules.cs`](Assets/_Project/Scripts/Combat/Domain/DamageCalculatorRules.cs) | 6 EditMode cases; combined current suite is 38/38 |

## 7. Rules for future status updates

1. Never write `100% complete`, `production-ready`, `playable`, or `PASS` from code inspection alone.
2. Every automated test count must name the test platform, result artifact, Unity version, Git revision, and execution date.
3. Every implemented mechanic must link to current source and, where applicable, an automated acceptance test.
4. Mark a GDD rule as implemented only when its full contract is present; visual placeholders or similarly named fields are not enough.
5. If code intentionally diverges from a LOCK rule, create a Director decision first. Do not silently update documentation after the fact.
6. Keep `Ban_Giao.md` synchronized with this registry. `Guideline.md` describes the current prototype interaction model and must not override the GDD.

## 8. Current release classification

**Battle-mechanics prototype — buildable, not a verified Funding Demo vertical slice.**

The next production gate is PlayMode evidence for the Instant Special checkpoint, including cancel/no-cost behavior, duplicate-Weakpoint rejection, charged Nhập Hồn persistence, cooldown timing, disabled-button keyboard behavior, and the return to Player Turn after Toàn Thức. That is followed by automated damage-pipeline verification, stable character/skill/item IDs, and the boss multi-entity/HP-gate contract. XIII's authored finisher and presentation must be completed with that boss contract. A full combat smoke test and responsive UI pass remain required before classifying the prototype as a verified combat slice.
