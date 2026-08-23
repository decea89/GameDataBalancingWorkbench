# GameDataBalancingWorkbench

Objetivo: herramienta WPF para editar y validar el balance de unidades de un RTS/4X ficticio.

MVP: importar units.json, editar unidades, métricas en vivo, reglas de validación, gráficas.

No incluido inicialmente: IA, SQLite, edificios, tech tree, multijugador, simulación compleja.

Stack: WPF, .NET moderno, MVVM, CommunityToolkit.Mvvm, System.Text.Json, xUnit.

Criterio de éxito: cambiar una stat actualiza métricas, warnings y gráfica sin reiniciar la app.