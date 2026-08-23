# BalanceForge – Copilot Instructions

## Product and user focus

BalanceForge is a designer-facing desktop content-authoring tool for a fictional tactical RPG / RTS.

The primary user is a game designer, not a programmer. Every workflow should make it easy to:

- Create and edit gameplay content safely.
- Understand validation errors without reading source code.
- Inspect derived balance metrics and understand their assumptions.
- Navigate related game-data entities as the project grows.
- Compare changes and confidently export validated data.

Prioritize clarity, responsiveness, keyboard-friendly workflows, safe editing and actionable validation messages over technical novelty.

## MVP scope

The first MVP supports:

- Loading and saving `units.json`.
- Browsing and filtering a unit roster.
- Editing unit definitions in a WPF DataGrid and inspector.
- Live derived metrics: DPS, total cost, DPS per cost and simplified effective health.
- Explainable validation issues.
- A clear dirty state and explicit, safe save workflow.

A unit includes:

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

Do not add features outside this MVP unless explicitly requested.

Planned post-MVP features are undo/redo, data diffing, relationships between units/abilities/effects, charts, a deterministic simulator, CLI validation and engine/pipeline integrations. Do not implement planned features prematurely.

## Technology

- C# and .NET 8 or later.
- WPF desktop application with XAML.
- MVVM using `CommunityToolkit.Mvvm`.
- `System.Text.Json` for serialization.
- xUnit for tests.
- `Microsoft.Extensions.DependencyInjection` for the composition root.
- Serilog for structured logging when logging is required.

## Architecture

Keep responsibilities separated:

- `BalanceForge.Domain`: pure models, enums, value objects and business-rule contracts. No WPF, file-system, JSON, DI or UI dependencies.
- `BalanceForge.Application`: use cases, metric calculators and validation services.
- `BalanceForge.Infrastructure`: JSON persistence, file-system access and logging implementations.
- `BalanceForge.Desktop`: WPF Views, ViewModels, bindings, UI-specific services and dependency composition.
- `BalanceForge.*.Tests`: xUnit tests for domain and application logic.

Prefer small, focused classes and explicit interfaces at external boundaries.

## Coding rules

- Enable and respect nullable reference types.
- Prefer async APIs for file I/O.
- Keep ViewModels thin; business logic belongs outside the WPF project.
- Avoid code-behind except for strictly view-only concerns.
- Treat game-data files as user-authored content. Never overwrite files without an explicit user action.
- Preserve unknown JSON fields where practical; do not silently discard content during a save operation.
- Do not silently change public models, JSON schema, calculations or validation behavior.
- Validation messages must identify the affected entity, explain the rule in plain language and provide a useful suggested action where possible.
- Calculated balance metrics are diagnostic signals, not objective balance truth. Label and present them accordingly.
- Design user-edit operations so undo/redo can be added later without rewriting application logic.
- Support keyboard navigation and large data sets. Avoid modal dialogs for routine editing workflows.
- Add or update tests whenever business logic, calculations, serialization or validation changes.
- Before declaring work complete, ensure the solution builds and relevant tests pass.

## Explicitly avoid unless requested

Do not introduce any of the following prematurely:

- MediatR
- CQRS
- Entity Framework Core
- SQLite
- Plugin systems
- AI/LLM features
- Network services
- Generic repositories
- Extra UI frameworks
- Over-engineered abstractions

Use the simplest solution that preserves the stated architecture and remains testable.

## Working style

- Before editing multiple files, briefly state the intended changes and affected layers.
- Make the smallest change that solves the requested task.
- Do not refactor unrelated code.
- Do not create placeholders, TODO-only implementations or speculative infrastructure.
- Flag ambiguities and architectural trade-offs instead of guessing.
- When adding a rule or calculation, include focused unit tests, including relevant edge cases.
- When presenting code changes, explain how to run or verify them.
