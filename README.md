# Parchis2D in Unity

## 🎮 Play the Game

| WEB | Android | iOS |
|-|-|-|
| <a href="https://play.unity.com/en/games/a3ba999e-7d04-4c1a-a5e0-fcd3c612854c/parchis2d-web"><img src="https://img.shields.io/badge/Web-Unity_Play-000000?logo=unity&logoColor=white" width="264" style="filter: invert(0);" /><br/></a> | <strong>COMING SOON!</strong> | <strong>COMING SOON!</strong> |




## Main Scene View

![Main Game View](https://github.com/benny-jon/Parchis2D/blob/main/RepoImages/game_main_view.png "Optional title")

## Architecture Overview

This project is organized into clear layers for input, board data, game rules, and turn/state control.

Below is a high-level explanation of each class and how they interact.

## Class Summaries

### BoardDefinition
Defines board layout constants (track length, home rows) and provides helper methods to get each player’s start, home-entry and home tiles.

### BoardTile
A simple data class representing a tile’s index, type (normal, safe, start etc.) and the player who owns it.

### BoardView
Handles world positions for tiles and spawn points, draws colored gizmos for tile types in the scene, and auto-assigns positions from the editor.

### Clickable2D
Abstract `MonoBehaviour` that can be clicked; subclasses override `OnClickDown`/`OnClickUp` to react to input.

### InputController
Uses Unity’s Input System to detect mouse/touch input, raycast into the scene and dispatch click events to `Clickable2D` objects.

### Piece
Represents a player's pawn, tracks its owner and current tile, provides methods to move or reset, and displays move hints.

### DiceButton
A button derived from `Clickable2D` that triggers a dice roll via the `GameManager`.

### BoardRules
Encapsulates Parchís game logic by determining valid moves, safe tiles, blockades, captures, progress scores and movement paths.

### GameStateMachine
Manages game phases, dice rolls, bonus moves, move resolution, repeated turns on doubles, and transitions between players.

### GameManager
Unity `MonoBehaviour` that wires together board rules, the state machine, input, UI, animations and sound, resets pieces, and updates player HUDs.

### MoveOption
Represents a candidate move including the piece, target tile, step count, dice usage and bonus index, used for selecting moves.

### MoveResult
Indicates the outcome of a move (invalid, normal, capture, blocked by blockade or reach home) and includes the resulting tile or captured piece.

### AnimationManager
Performs coroutines that animate pieces resetting to their base or moving along a path and calls callbacks when finished.

### SoundManager
Stores audio clips and plays sounds for dice rolls, clicks, moves ending, captures, returning to start, reaching home, winning and UI clicks.

### ParchisUI
Manages the orientation-specific UI roots, shows dice values and move/roll hints per player, clears hints, updates orientation and displays game-over text.

### GamePhase (enum)
Defines the state machine’s phases: waiting to roll, waiting to move, animating a move or game over.

### ArrayUtils
Provides a utility to convert integer arrays to formatted strings for debugging purposes.

