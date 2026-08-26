# Feature Plan: Explainable Outlier Diagnostics

## Problem

Designers can inspect raw metrics, but they still have to mentally determine which units
deviate from the rest of their tier. This slows down roster reviews and makes suspicious
values easy to miss.

## Goal

Add a deterministic diagnostic layer that highlights unusual Cost, DPS and Effective Health
values and explains the largest deviation in plain language.

## Scope

### P0

- Calculate tier medians for Total Cost, DPS and Effective Health.
- Calculate each unit's percentage deviation from those medians.
- Classify units as Balanced, Watch or Outlier using documented thresholds.
- Show the classification and strongest deviation on each unit card.
- Keep analysis independent from WPF and covered by unit tests.

### P1

- Show the total number of flagged units in the overview header.
- Keep benchmarks stable when UI filters hide units.

## Non-goals

- Automatically changing unit values.
- Claiming that a statistical outlier is objectively unbalanced.
- Machine learning, simulation or historical trend analysis.
- User-configurable formulas in this iteration.

## Acceptance Criteria

- Given at least two units in a tier, when analysis runs, then every unit receives deviations
  against the same tier medians.
- Given a largest absolute deviation below 35%, when classified, then the unit is Balanced.
- Given a deviation from 35% up to 74.99%, when classified, then the unit is Watch.
- Given a deviation of at least 75%, when classified, then the unit is Outlier.
- Given a tier with one unit, when analysis runs, then the unit reports insufficient peers
  instead of a misleading outlier.
- Given active role or tier filters, when cards update, then diagnostics still use the full
  loaded roster as the benchmark.

## Test Strategy

- Median calculation for odd and even groups.
- Classification boundaries.
- Strongest-deviation explanation.
- Single-unit tier fallback.
- Build the full WPF solution and run all non-WPF tests.
