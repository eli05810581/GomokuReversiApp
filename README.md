# GomokuReversiApp

A C# console application for playing multiple variants of Gomoku and Reversi.

**Course:** IFN584 – Object-Oriented Design and Development  
**University:** Queensland University of Technology (QUT)   
**Programming Language:** C#  
**Framework:** .NET 10

## 1. Project Overview

GomokuReversiApp is a console-based board game application developed as part of IFN584 – Object-Oriented Design and Development.

The application supports multiple variations of Gomoku and Reversi, different player modes, and computer-controlled opponents.

The project demonstrates object-oriented design principles through the separation of game logic, move processing, player management, artificial intelligence, and game state management.

## 2. Game Variants

### Gomoku

- **Standard Gomoku:** Players take turns placing stones on the board. The objective is to form five consecutive stones.
- **GomokuPlus:** Extends standard Gomoku with special stones, including Heavy Stones and Erasers.
- **GomokuFog:** Introduces limited board visibility using a fog-of-war mechanism.

### Reversi

- **Standard Reversi:** Players place discs to capture and flip their opponent's discs.
- **Anti Reversi:** A variation of Reversi with a different winning objective.
- **Corner Reversi:** A variation with special rules involving corner positions.

## 3. Game Modes

The application supports three interactive game modes:

1. **Human vs Human:** Two human players take turns.
2. **Human vs Dumb AI:** A human player competes against a basic computer opponent.
3. **Human vs Smart AI:** A human player competes against an AI opponent using game-specific move selection strategies.

## 4. Requirements

To build and run the application, the following software is required:

- .NET 10 SDK
- A terminal or command-line interface
- Visual Studio Code, Visual Studio, or another compatible C# development environment (optional)

## 5. Installation and Execution

Clone the repository from GitHub or download and extract the project ZIP file.

Open a terminal in the folder containing `GomokuReversiApp.csproj`.

Build the project:

```bash
dotnet build
```

Run the application:

```bash
dotnet run
```

The main menu provides the following options:

```text
1. New Game
2. Load Game
3. Exit
```

To start a new game:

1. Select New Game.
2. Choose Gomoku or Reversi.
3. Select the game variant.
4. Select the game mode.
5. Follow the on-screen instructions to play.

## 6. Game Commands

The following commands are available during gameplay:

| Command | Description |
|---|---|
| `help` | Display game instructions and available commands |
| `undo` | Undo the previous turn |
| `redo` | Redo a previously undone turn |
| `save filename.json` | Save the current game to a JSON file |
| `load` | Load a previously saved game |
| `exit` | Exit the current game session |
| `quit` | Exit the current game session |

When using `load`, the application prompts the player to enter the saved file name.

### Gomoku Move Examples

| Command | Description |
|---|---|
| `O5:5` | Place an ordinary stone at row 5, column 5 |
| `H5:5` | Place a Heavy Stone in GomokuPlus |
| `E5:5` | Use an Eraser in GomokuPlus |

Special stones are subject to the rules and availability restrictions of GomokuPlus.

### Reversi Move Examples

| Command | Description |
|---|---|
| `P3:4` | Place a disc at row 3, column 4 |
| `PASS` | Pass a turn when permitted by the game rules |

Only legal moves are accepted.

## 7. Artificial Intelligence

The application provides two AI difficulty levels.

**Dumb AI**

Uses a basic move selection strategy to choose legal moves.

**Smart AI**

Uses game-specific strategies to evaluate available moves and select suitable actions.

AI behaviour varies depending on the selected game variant.

## 8. Object-Oriented Design

The application separates responsibilities into different classes and interfaces to support maintainability and extensibility.

Key design elements include:

- **Factory Pattern:** Creates the appropriate game configuration and components for each variant.
- **Strategy Pattern:** Supports different AI move selection behaviours.
- **Command Pattern:** Encapsulates user actions such as moves, undo, redo, save, and load.
- **Interfaces and Polymorphism:** Allow game-specific implementations to share common contracts.
- **Encapsulation:** Keeps board management, game rules, and player behaviour in dedicated components.

## 9. Project Structure

| File | Responsibility |
|---|---|
| `Program.cs` | Application entry point and game menus |
| `Game.cs` | Main game flow and turn management |
| `Board.cs` | Board representation and operations |
| `Players.cs` | Human and AI player management |
| `Moves.cs` | Move representations |
| `MoveParsers.cs` | Parsing game-specific moves |
| `MoveProcessors.cs` | Move validation and application |
| `MoveStratergies.cs` | AI move selection strategies |
| `GameFactories.cs` | Game variant creation |
| `GameOverCheckers.cs` | Game completion and winner detection |
| `SaveFileManager.cs` | Game saving and loading |
| `InputParser.cs` | Parsing user commands |
| `Commands.cs` | Command implementations |
| `VisibilityModels.cs` | Board visibility behaviour |
| `HelpProviders.cs` | Game-specific instructions |
| `GameState.cs` | Game state representation |
| `diagrams/` | Project design diagrams |

## 10. Save and Load

The application supports saving and restoring game progress using JSON files.

The save/load functionality allows players to preserve game state and continue a previously saved game.

Example:

```text
save testgame.json
```

To restore the game:

```text
load
```

Enter `testgame.json` when prompted.

## 11. Testing

The application has been manually tested through its console interface.

Testing activities include:

- Legal and invalid move handling
- Gomoku winner detection
- Reversi disc flipping
- GomokuPlus Heavy Stone and Eraser functionality
- Human vs Human gameplay
- Dumb AI and Smart AI move execution
- Save and Load
- Undo and Redo
- Reversi PASS behaviour
- Game completion and winner detection
- Corner Reversi gameplay

These tests verify core functionality through representative gameplay scenarios. They do not constitute exhaustive automated testing.

## 12. Design Documentation

The `diagrams/` directory contains design diagrams associated with the application.

These diagrams support understanding of the program's structure and interactions between components.

## 13. Academic Information

This project was developed for **IFN584 – Object-Oriented Design and Development** at **Queensland University of Technology (QUT)** in 2026.

The application demonstrates the practical application of object-oriented programming concepts and software design patterns in a multi-variant board game system.