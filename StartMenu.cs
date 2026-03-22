using System.Text;

namespace HangmanGame;

public class StartMenu
{
    private SurvivalMode _survivalMode;
    private ClassicMode _classicMode;
    private GameMode _gameMode;

    private int lastWidth = Console.WindowWidth;
    private int lastHeight = Console.WindowHeight;

    // ASCII logo used on the start screen
    private readonly string[] logo = new[]
    {
        @" _   _                                         ",
        @"| | | | __ _ _ __   __ _ _ __ ___   __ _ _ __  ",
        @"| |_| |/ _` | '_ \ / _` | '_ ` _ \ / _` | '_ \ ",
        @"|  _  | (_| | | | | (_| | | | | | | (_| | | | |",
        @"|_| |_|\__,_|_| |_|\__, |_| |_| |_|\__,_|_| |_|",
        @"                   |___/                       "
    };

    // Displays the start menu and handles user flow
    public void DisplayMenu()
    {
        SetupConsole();

        // 1. Show an animation until a key is pressed
        WaitForPlayer();

        // 2. Ask which game mode to play
        GameMode selectedMode = GetGameMode();

        // 3. Start the selected game
        StartGame(selectedMode);
    }

    private void SetupConsole()
    {
        Console.Title = "Hangman";
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
    }

    // Wait loop that draws an animated logo and blinking prompt
    private void WaitForPlayer()
    {
        double phase = 0;
        Console.Clear();

        while (!Console.KeyAvailable)
        {
            // 1. Resize check: prevent ghosting when window is resized
            HandleResize();

            // 2. Draw the logo
            DrawAnimatedLogo(phase);

            // 3. Draw blinking prompt
            BlinkPressAnyKey(phase);

            phase -= 0.15;
            Thread.Sleep(50);
        }
        Console.ReadKey(true); // consume the key press
    }

    private void HandleResize()
    {
        if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
        {
            Console.Clear(); // Clear completely on resize
            lastWidth = Console.WindowWidth;
            lastHeight = Console.WindowHeight;
        }
    }

    private void DrawAnimatedLogo(double phase)
    {
        int logoWidth = logo.Max(line => line.Length);
        int paddingX = Math.Max(0, (Console.WindowWidth - logoWidth) / 2);
        int paddingY = Math.Max(0, (Console.WindowHeight - logo.Length) / 2 - 2);

        var outputBuffer = new StringBuilder();
        for (int i = 0; i < paddingY; i++) outputBuffer.AppendLine();

        string paddingStr = new string(' ', paddingX);

        foreach (var line in logo)
        {
            outputBuffer.Append(paddingStr);
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] != ' ')
                {
                    double colorPos = (x * 0.15) + phase;
                    int r = (int)(Math.Sin(colorPos) * 127 + 128);
                    int g = (int)(Math.Sin(colorPos + 2) * 127 + 128);
                    int b = (int)(Math.Sin(colorPos + 4) * 127 + 128);
                    outputBuffer.Append($"\x1b[38;2;{r};{g};{b}m{line[x]}");
                }
                else outputBuffer.Append(" ");
            }
            outputBuffer.AppendLine();
        }

        Console.SetCursorPosition(0, 0);
        Console.Write(outputBuffer.ToString());
    }

    private void BlinkPressAnyKey(double phase)
    {
        string prompt = "Press any key . . .";
        int promptPadding = Math.Max(0, (Console.WindowWidth - prompt.Length) / 2);

        // Use a sine wave for blinking effect
        bool isVisible = Math.Sin(phase * 2) > 0;

        // Position cursor under the logo (logo height + padding)
        int logoY = Math.Max(0, (Console.WindowHeight - logo.Length) / 2 - 2) + logo.Length + 2;

        // Ensure we don't write outside the window
        if (logoY < Console.WindowHeight)
        {
            Console.SetCursorPosition(promptPadding, logoY);
            if (isVisible)
                Console.Write("\x1b[1;37m" + prompt + "\x1b[0m"); // White
            else
                Console.Write(new string(' ', prompt.Length)); // overwrite with spaces
        }
    }

    // Ask the player to choose a game mode
    private GameMode GetGameMode()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n\n\x1b[1m  Choose a Gamemode:\x1b[0m\n");
            Console.WriteLine("  [1] Classic");
            Console.WriteLine("  [2] Survival");

            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1) return GameMode.Classic;
            if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2) return GameMode.Survival;
        }
    }

    private void StartGame(GameMode gameMode)
    {
        Console.Clear();
        _gameMode = gameMode;
        if (_gameMode == GameMode.Survival)
        {
            _survivalMode = new SurvivalMode();
            _survivalMode.StartSurvivalGameAsync();
            return;
        }
        _classicMode = new ClassicMode();
        _classicMode.StartClassicGame();
    }
}