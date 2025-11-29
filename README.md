# Parchis2D in Unity

## Main Scene View

![Main Game View](https://github.com/benny-jon/Parchis2D/blob/main/RepoImages/game_main_view.png "Optional title")

## Architecture Overview

This project is organized into clear layers for input, board data, game rules, and turn/state control.

Below is a high-level explanation of each class and how they interact.

## Class Summaries

### BoardDefinition
Stores the logical definition of every tile on the board (type, owner, index).

### BoardTile
Represents a single tile’s metadata such as its index, type, and owning player.

### BoardView
Provides the world-space positions of tiles and draws gizmos for visual debugging in the Scene view.

### Clickable2D
Base class for any in-scene object that can be clicked using the global Input System raycasts.

### InputController
Handles mouse/touch input using the new Input System and dispatches click events to Clickable2D objects.

### Piece
Represents a player’s pawn and moves visually on the board when commanded by the game rules.

### DiceButton
A Clickable2D object that triggers a dice roll through the GameManager.

### BoardRules
Contains all logic for computing valid movement paths across the board, including base, loops, and home rows.

### GameStateMachine
Controls turn order, game phases, dice interactions, and move resolution for the entire game flow.

### GameManager
Central glue layer connecting input, rules, state machine, board view, and assets in the Unity scene.
