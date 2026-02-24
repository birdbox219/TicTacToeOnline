# Adding New Game Modes — Complete Guide

## System Overview

The board system is data-driven. Each variant is defined by a **BoardConfig** ScriptableObject. The game reads the selected config at runtime and dynamically generates the grid, win lines, and visuals.

```
BoardConfig (data) → GameManager (logic) → GridSpawner (visuals)
                          ↑
               LobbyManager.SelectedGameMode
```

---

## Step-by-Step: Adding a New Board Variant

### Step 1: Create a BoardConfig Asset

1. Right-click in `Assets/BoardConfigs/` → **Create → ScriptableObject → BoardConfig**
2. Fill in the fields:

| Field | Description | Example (4×4) |
|-------|-------------|----------------|
| `modeName` | Display name (must match Step 3) | `"Board 4x4"` |
| `width` | Number of columns | `4` |
| `height` | Number of rows | `4` |
| `winLength` | How many in a row to win | `4` |
| `cellSpacing` | World-space distance between cells | `3.1` |
| `validCells` | Boolean array (size = width × height) | All `true` for rectangular |

#### Understanding `validCells`

This array defines which cells exist. Index = `y * width + x`.

For a **rectangular** board (all cells valid), right-click the asset → **Reset Valid Cells** to auto-fill.

For a **shaped** board (like Pyramid), set specific cells to `false`:
```
Example: 5×3 Pyramid
Row 2 (top):     _  _  ✓  _  _     → indices 10-14: F F T F F
Row 1 (mid):     _  ✓  ✓  ✓  _     → indices  5-9:  F T T T F
Row 0 (bottom):  ✓  ✓  ✓  ✓  ✓     → indices  0-4:  T T T T T
```

### Step 2: Add to GameMode Enum

In `LobbyManager.cs`, add your new value:

```csharp
public enum GameMode {
    Classic3x3,
    PyramidXO,
    Board4x4       // ← ADD THIS
}
```

### Step 3: Update the GameMode → Config Mapping

In `GameManager.cs` → `SetBoardConfig()`, add your case:

```csharp
string targetName = gameMode switch
{
    LobbyManager.GameMode.Classic3x3 => "Classic 3x3",
    LobbyManager.GameMode.PyramidXO  => "Pyramid XO",
    LobbyManager.GameMode.Board4x4   => "Board 4x4",   // ← ADD THIS
    _ => "Classic 3x3"
};
```

> **IMPORTANT**: The string must exactly match the `modeName` field in your BoardConfig asset.

### Step 4: Update the Cycle Logic (2 files)

**LobbyManager.cs** → `ChangeGameMode()`:
```csharp
switch (gameMode) {
    default:
    case GameMode.Classic3x3:
        gameMode = GameMode.PyramidXO;
        break;
    case GameMode.PyramidXO:
        gameMode = GameMode.Board4x4;        // ← CHAIN TO NEW
        break;
    case GameMode.Board4x4:                  // ← ADD CASE
        gameMode = GameMode.Classic3x3;      //    cycles back
        break;
}
```

**LobbyCreateUI.cs** → `gameModeButton.onClick` (same pattern):
```csharp
switch (gameMode) {
    default:
    case LobbyManager.GameMode.Classic3x3:
        gameMode = LobbyManager.GameMode.PyramidXO;
        break;
    case LobbyManager.GameMode.PyramidXO:
        gameMode = LobbyManager.GameMode.Board4x4;        // ← CHAIN
        break;
    case LobbyManager.GameMode.Board4x4:                  // ← ADD
        gameMode = LobbyManager.GameMode.Classic3x3;
        break;
}
```

### Step 5: Wire the Asset in the Inspector

1. Select the **GameManager** GameObject in the scene
2. Find the `Board Configs` array
3. Increase the array size and drag your new `BoardConfig` asset into the new slot

### Step 6: Test

1. Hit Play
2. In the lobby creation screen, click the Game Mode button to cycle to your new variant
3. Verify the correct grid spawns when the game starts

---

## Quick Reference: Files You Touch

| File | What to change |
|------|----------------|
| `Assets/BoardConfigs/` | Create new `.asset` |
| `LobbyManager.cs` | Add enum value + update cycle |
| `LobbyCreateUI.cs` | Update cycle (same pattern) |
| `GameManager.cs` | Add mapping in `SetBoardConfig()` |
| **Scene Inspector** | Add asset to `boardConfigs[]` array |

---

## Tips

- **Rectangular boards**: Just set all `validCells` to `true` (or use Reset Valid Cells context menu)
- **Shaped boards**: Use`validCells` to mask out cells — the win-line generator automatically skips invalid cells
- **Win length**: Can differ from board size (e.g., 4×4 board with 3-in-a-row)
- **Cell spacing**: Adjust `cellSpacing` if larger boards look too spread out or too cramped
- **Backup**: Original scripts are in `Assets/_Backup_Original/` if you ever need to compare
