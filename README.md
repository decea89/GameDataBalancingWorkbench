# BalanceForge

> A designer-facing desktop workbench for authoring, validating and analysing gameplay balance data.

BalanceForge is a portfolio project for a fictional tactical RPG / RTS. It models a real production workflow: designers define gameplay entities, inspect derived balance metrics, receive explainable validation feedback, compare data changes and export validated content for use by a game.

The project is intentionally designed as a maintainable internal-tool style application rather than a generic CRUD demo. Its primary user is a game designer, not a programmer.

## Goals

BalanceForge demonstrates:

- WPF and XAML desktop UI design for data-heavy creative workflows.
- MVVM architecture and maintainable modern .NET application structure.
- Designer-facing game-content authoring, validation and feedback loops.
- Safe editing workflows: explicit save operations, dirty-state feedback, validation and import/export boundaries.
- Extensible game-data rules that are independent from the user interface.
- A foundation for later engine/editor, CI and DCC-facing pipeline integrations.

## MVP

The first release focuses on unit balance data.

### Supported workflow

1. Load a `units.json` file.
2. Browse and filter a roster of units.
3. Edit a selected unit using a DataGrid and detail inspector.
4. See derived balance metrics update immediately.
5. Review validation issues that identify the affected unit, explain the rule and suggest an action.
6. Save validated data explicitly.

### Unit data

A unit definition includes:

- `Id`
- `DisplayName`
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
│   └── units.json
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

## Development principles

- Treat game-data files as user-authored content; never overwrite them without an explicit user action.
- Keep business logic outside WPF ViewModels and code-behind.
- Prefer small, testable services with clear responsibilities.
- Keep validation deterministic, explainable and actionable.
- Use asynchronous APIs for file I/O.
- Keep the interface responsive and suitable for keyboard-oriented data-entry workflows.
- Add or update tests when business rules or calculations change.

## Roadmap

### MVP

- [ ] Load and save `units.json`
- [ ] Unit roster DataGrid with filters
- [ ] Unit detail inspector
- [ ] Live balance metrics
- [ ] Validation issue panel
- [ ] Dirty-state and explicit save workflow
- [ ] Unit and application-layer tests

### Post-MVP

- [ ] Undo/redo for content edits
- [ ] Compare two data files and present a readable diff
- [ ] Unit, ability, effect and tag relationships
- [ ] Charts for tier, cost and effectiveness analysis
- [ ] Deterministic matchup simulation
- [ ] Headless CLI validation for CI
- [ ] JSON, Markdown and SARIF reports
- [ ] Import helpers for design spreadsheets

## Non-goals

BalanceForge is not intended to:

- Be a complete game engine or Unity/Unreal editor replacement.
- Decide what constitutes good balance automatically.
- Modify game data silently.
- Introduce database, AI, plugin or network infrastructure before it solves a demonstrated product need.

## License

This project is published for portfolio and learning purposes. License selection is pending.
