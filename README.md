# BalanceForge

> A designer-facing desktop workbench for authoring, validating and analysing gameplay balance data.

BalanceForge is a portfolio project for a fictional tactical RPG / RTS. It models a real production workflow: designers define gameplay entities, inspect derived balance metrics, receive explainable validation feedback, compare data changes and export validated content for use by a game.

The project is intentionally designed as a maintainable internal-tool style application. Its primary user is a game designer, not a programmer.

<img width="2559" height="1005" alt="image" src="https://github.com/user-attachments/assets/aff27a87-492c-4fb1-9468-0ae0697007eb" />


## Quick Start

1. Launch the application.
2. Click **Select File** and navigate to `samples/units.json`.
3. Click **Load** to populate the roster and its illustrated unit overview.
4. Select a unit in the grid to view its stats in the inspector.
5. **Ctrl+click** another unit to compare side-by-side.
6. Edit any stat in the inspector; see metrics and validation feedback update in real-time.
7. Click **Undo** (or Ctrl+Z) to revert changes, or **Redo** (Ctrl+Y) to reapply them.
8. Click **Save** when ready; unsaved changes are protected by a confirmation dialog.
9. Click **Compare Snapshot** and choose another roster JSON to review added, removed and field-level balance changes.

For a ready-made diff demo, load `samples/units.json` as the current roster and select
`samples/units-baseline.json` from **Compare Snapshot**. The two files include modified,
added and removed units so every diff state is visible immediately.

## Demo

For a complete walkthrough of the MVP workflow, see [DEMO_RECORDING.md] (WIP)

To view a live demonstration:
1. Download and run the application
2. Follow the Quick Start steps above with `samples/units.json`
3. The interface will show a balance chart, roster filters, unit inspector, validation panel and comparison view all working together

## Goals

BalanceForge demonstrates:

- WPF and XAML desktop UI design for data-heavy creative workflows.
- MVVM architecture and maintainable modern .NET application structure.
- Designer-facing game-content authoring, validation and feedback loops.
- Safe editing workflows: explicit save operations, dirty-state feedback, validation and import/export boundaries.
- Extensible game-data rules that are independent from the user interface.
- A foundation for later engine/editor, CI and DCC-facing pipeline integrations.

## MVP (Completed)

The first release focuses on unit balance data and is feature-complete.

### Supported workflow

1. Load a `units.json` file (or start with `samples/units.json`).
2. Browse and filter a roster of units by Role and Tier.
3. Select a unit to view detailed stats in the inspector.
4. Edit any unit property; metrics and validation update in real-time.
5. Compare two units side-by-side: select one unit, then Ctrl+click another.
6. Review validation issues that identify the affected unit, explain the rule and suggest an action.
7. Undo/Redo any change with Ctrl+Z / Ctrl+Y.
8. View illustrated unit cards showing Total Cost, DPS and Effective Health across all displayed units.
9. Review explainable outlier diagnostics against each unit's tier median.
10. Compare the current in-memory roster against another JSON snapshot with readable field deltas.
11. Save validated data explicitly when ready.

### Unit data

A unit definition includes:

- `Id`
- `DisplayName`
- `ImagePath` (optional, relative to the roster JSON file or absolute)
- `Role`
- `Tier`
- `Health`
- `Damage`
- `AttacksPerSecond`
- `Armor`
- `Range`
- `WoodCost`
- `GoldCost`
- `PopulationCost`
- `ProductionTimeSeconds`

### Derived metrics

The initial metrics are diagnostic signals, not claims of objectively correct game balance:

- Damage per second: `Damage × AttacksPerSecond`
- Total cost: `WoodCost + GoldCost`
- DPS per cost
- Simplified effective health
- Tier and role-relative balance indicators
- Explainable tier-median outlier diagnostics for Cost, DPS and Effective Health

### Validation rules

The initial validation engine checks:

- Health, damage, costs and production time cannot be negative.
- A non-support unit cannot have zero DPS.
- Tier must be between 1 and 4.
- A Tier 2+ unit cannot have a lower total cost than a Tier 1 unit of the same role unless an explicit exception is defined.

Every issue must include a severity, rule identifier, affected entity, plain-language explanation and suggested action when practical.

## Architecture

```text
BalanceForge.sln
│
├── src/
│   ├── BalanceForge.Domain/          # Models, enums, value objects and rules
│   ├── BalanceForge.Application/     # Use cases, metrics and validation services
│   ├── BalanceForge.Infrastructure/  # JSON persistence, filesystem and logging
│   └── BalanceForge.Desktop/         # WPF Views, ViewModels and composition root
│
├── tests/
│   ├── BalanceForge.Domain.Tests/
│   └── BalanceForge.Application.Tests/
│
├── samples/
│   ├── units.json
│   ├── units-baseline.json           # Earlier balance pass for snapshot diff demos
│   └── images/                       # Optional unit illustrations referenced by imagePath
│
└── docs/
```

### Layer responsibilities

| Layer | Responsibility |
|---|---|
| `Domain` | Pure game-data models, enums, value objects and rule contracts. No WPF, JSON, file-system or DI dependencies. |
| `Application` | Metric calculation, content validation and use cases that orchestrate domain logic. |
| `Infrastructure` | JSON serialization, file-system operations and logging implementations. |
| `Desktop` | WPF views, ViewModels, bindings, UI-specific services and dependency composition. |

## Technology

- C# and .NET 8 or later
- WPF and XAML
- MVVM with `CommunityToolkit.Mvvm`
- `System.Text.Json`
- `Microsoft.Extensions.DependencyInjection`
- xUnit for tests
- Serilog for structured logging

## Data example

```json
{
  "units": [
    {
      "id": "knight",
      "displayName": "Knight",
      "imagePath": "images/knight.png",
      "role": "Cavalry",
      "tier": 2,
      "health": 180,
      "damage": 18,
      "attacksPerSecond": 1.1,
      "armor": 4,
      "range": 1.5,
      "woodCost": 0,
      "goldCost": 90,
      "populationCost": 2,
      "productionTimeSeconds": 28
    }
  ]
}
```

`imagePath` is optional. Relative paths are resolved from the folder containing the loaded
roster file, which keeps data packs portable. If the file is missing or the property is
omitted, the unit card displays a placeholder with the unit's initial.

## Development principles

- Treat game-data files as user-authored content; never overwrite them without an explicit user action.
- Keep business logic outside WPF ViewModels and code-behind.
- Prefer small, testable services with clear responsibilities.
- Keep validation deterministic, explainable and actionable.
- Use asynchronous APIs for file I/O.
- Keep the interface responsive and suitable for keyboard-oriented data-entry workflows.
- Add or update tests when business rules or calculations change.

## Roadmap

### MVP (✅ Completed)

- [x] Load and save `units.json`
- [x] Unit roster DataGrid with filters by Role and Tier
- [x] Unit detail inspector with live edits
- [x] Live balance metrics (DPS, Total Cost, DPS/Cost, Effective Health)
- [x] Validation issue panel with explainable, actionable feedback
- [x] Dirty-state and explicit save workflow
- [x] Undo/Redo for all content edits (Ctrl+Z / Ctrl+Y)
- [x] Illustrated unit overview for visual comparison (Total Cost, DPS, Effective Health)
- [x] Two-unit comparison panel with Ctrl+click selection
- [x] Unit and application-layer tests
- [x] Expanded eight-unit sample roster (`samples/units.json`)
- [x] Optional unit artwork loaded through portable JSON paths
- [x] Explainable outlier diagnostics with tier benchmarks

### Post-MVP

- [x] Compare two data files and present a readable, live-updating field diff
- [x] Unit, ability, effect and tag relationships
- [x] Tier and role-relative balance indicators
- [ ] Deterministic matchup simulation
- [ ] Headless CLI validation for CI
- [ ] JSON, Markdown and SARIF reports
- [ ] Import helpers for design spreadsheets
- [ ] Animation/visual feedback for edits and validation changes

## License

This project is published for portfolio and learning purposes. License selection is pending.
