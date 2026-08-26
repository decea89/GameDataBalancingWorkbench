# Feature Plan: Roster Snapshot Diff

## Problem Statement

Game designers often need to understand what changed between two balance passes, but comparing JSON by hand hides gameplay-relevant changes inside formatting noise. BalanceForge should turn two roster files into a concise, field-level review that can be read without leaving the editor.

## Goals

- Let a designer choose another roster as a baseline while keeping the current roster open.
- Classify units as added, removed, modified or unchanged using stable unit IDs.
- Show readable field-level changes and numeric deltas.
- Refresh the diff immediately after current in-memory edits, undo and redo.
- Keep comparison logic independent from WPF and covered by unit tests.

## Non-Goals

- No Git integration or version-control history.
- No three-way merge, conflict resolution or automatic file modification.
- No patch export, report generation or CI integration.
- No historical database or persistent snapshots.

## User Stories

- As a game designer, I want to compare my current roster with an earlier JSON file so that I can review a balance pass quickly.
- As a reviewer, I want to see which fields changed and by how much so that I can judge gameplay impact.
- As a designer editing live values, I want the snapshot diff to refresh immediately so that I can validate the intended delta before saving.

## Requirements

### P0

- Compare baseline and current rosters by case-insensitive unit ID.
- Detect added, removed, modified and unchanged units.
- Compare every persisted unit field.
- Show baseline value, current value and numeric delta where applicable.
- Keep the loaded baseline read-only.
- Handle cancel, invalid files and comparing a file with itself without losing current work.

### P1

- Display summary counts and a compact human-readable change summary per unit.
- Refresh modified rows after edits, undo and redo.
- Provide a clear action to close the snapshot comparison.

## Acceptance Criteria

- Given a current roster is loaded, when the designer selects a different valid roster, then a diff panel shows correct added, removed and modified counts.
- Given one unit has changed numeric and text fields, when the comparison is created, then each changed field shows baseline and current values and numeric fields include a signed delta.
- Given a snapshot comparison is active, when the designer edits, undoes or redoes a current value, then the corresponding diff row refreshes immediately.
- Given the file picker is cancelled, when the command returns, then the current roster and any existing comparison remain unchanged.
- Given the selected baseline is the current file, when comparison is requested, then an actionable error is shown and no diff is created.

## Test Strategy

- Unit-test identical, added, removed and modified rosters.
- Unit-test numeric delta and percentage calculations, including a zero baseline.
- Build the full solution with warnings treated normally.
- Run all existing test projects to catch regressions in load, validation, editing and persistence.

