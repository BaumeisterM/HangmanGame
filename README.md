# HangmanGame

A simple console-based Hangman game written in C#.

Features
- Classic mode: guess a single word with limited lives.
- Survival mode: guess as many words as possible within a time limit; correct guesses grant extra time and lives.

Requirements
- .NET 10 (project targets .NET 10)
- Visual Studio or dotnet CLI

How to run
1. Open the solution in Visual Studio and run the project, or use the dotnet CLI:

   ```powershell
   dotnet run --project "./HangmanGame.csproj"
   ```

2. The game will show a start screen. Press any key to continue and choose a game mode.

Files of interest
- `Program.cs` - application entry point
- `Game.cs` - base class that loads the word list
- `StartMenu.cs` - start screen and mode selection
- `ClassicMode.cs` - classic single-word hangman mode
- `SurvivalMode.cs` - timed survival mode with multiple words

Word list
Place a `Hangmanlist.txt` file in the `Content` folder next to the application binary. Words should be comma-separated. If the file is not present the game will use a small fallback list.
