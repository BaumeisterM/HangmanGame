using System.Diagnostics;

namespace HangmanGame;

// SurvivalMode implements a timed, successive-words hangman game.
// The player must guess as many words as possible before time runs out
// or they run out of lives. Correctly guessed words grant extra time
// and additional lives.
public class SurvivalMode : Game
{
    private readonly Random rnd = new Random();
    private string _secretWord;
    private CancellationTokenSource _cancellationTokenSource;
    private Stopwatch _stopwatch;
    private TimeSpan _extraTime;
    private readonly List<string> _guessedWords = new List<string>();
    private int _lives = 10;
    private int wordCount = 0;
    private int _usedLives = 0;

    public SurvivalMode() : base() { }

    // Starts the survival game loop asynchronously. This prepares timing
    // infrastructure, runs the game, and then prints final statistics.
    public async Task StartSurvivalGameAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _stopwatch = new Stopwatch();
        _extraTime = TimeSpan.Zero;

        // Start measuring elapsed time and run a background timer task
        _stopwatch.Start();
        var timerTask = RunTimerAsync(_cancellationTokenSource.Token);

        // Run the main synchronous game loop that reads console input
        GameLoop();

        // When the loop finishes stop measuring and cancel the timer
        _stopwatch.Stop();
        _cancellationTokenSource.Cancel();

        // Determine why the game ended and show an appropriate message
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        if (GetRemainingTime() <= TimeSpan.Zero) Console.WriteLine("--- TIME IS UP! ---");
        else if (_lives <= 0) Console.WriteLine("--- NO LIVES LEFT! ---");
        else if (_secretWord == null) Console.WriteLine("--- NO WORDS LEFT! YOU GUESSED THE WHOLE LIST! ---");
        else Console.WriteLine("--- GAME EXITED ---");
        Console.ResetColor();

        // Small pause to mimic loading of statistics
        Console.WriteLine("\nLoading statistics . . .");
        Thread.Sleep(2000);

        // Await the timer task cancellation (ignore OperationCanceledException)
        try { await timerTask; } catch (OperationCanceledException) { }

        // Output final statistics to the player
        Console.Clear();
        Console.WriteLine("========================");
        Console.WriteLine("=   FINAL STATISTICS   =");
        Console.WriteLine("========================");
        Console.WriteLine($"Total guessed words: {wordCount}");
        Console.WriteLine($"Guessed words: {string.Join(", ", _guessedWords)}");
        Console.WriteLine($"Total game duration: {GetTotalGameDuration()} seconds");
        Console.WriteLine($"Lives used: {_usedLives}");
        Console.WriteLine("========================");

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(); // Wait for user input before returning
    }

    // Main loop that drives the gameplay. It selects the next word,
    // displays the masked word, accepts user input, and applies game rules.
    private void GameLoop()
    {
        _secretWord = NextSecretWord();
        string hidden = HiddenWord(_secretWord);
        string incorrectLetters = "";

        while (true)
        {
            // End the loop if there are no more words
            if (_secretWord == null) break;

            // End if time ran out
            if (GetRemainingTime() <= TimeSpan.Zero) break;

            // Display current game state
            Console.Clear();
            Console.WriteLine($"Time remaining: {GetRemainingTime():mm\\:ss}");
            Console.Write($"Word: {hidden}\t| Lives: {_lives}\t| Incorrect: {incorrectLetters}\t| Guess letter (or type 'exit'): ");

            // If no key available, wait a short time so the timer updates visually
            if (!Console.KeyAvailable)
            {
                Thread.Sleep(500);
                continue;
            }

            // Read a full line of input and normalize to lower-case
            string? input = Console.ReadLine()?.ToLower();

            if (input == "exit") break; // Allow user to exit early

            if (_lives <= 0) break; // Safety: end if no lives left

            // Only accept single-letter guesses
            if (!string.IsNullOrEmpty(input) && input.Length == 1)
            {
                char guess = input[0];

                // Skip letters already tried
                if (incorrectLetters.Contains(input.ToUpper())) continue;

                // If the secret word does not contain the guessed letter, count a miss
                if (!_secretWord.Contains(input.ToUpper()))
                {
                    _lives--; // Decrement lives for wrong guess
                    incorrectLetters += input.ToUpper(); // Track incorrect letters
                }

                // Reveal correctly guessed letters in the hidden representation
                hidden = UpdateHiddenWord(hidden, _secretWord, input.ToUpper()[0]);

                // If the word is fully revealed handle success: grant time, lives and pick next word
                if (!hidden.Contains('_'))
                {
                    _usedLives++;
                    wordCount++;
                    _guessedWords.Add(hidden);
                    AddExtraTime(TimeSpan.FromSeconds(10)); // reward: +10 seconds

                    _secretWord = NextSecretWord();
                    hidden = HiddenWord(_secretWord);

                    incorrectLetters = ""; // reset incorrect letters for next word
                    _lives += 5; // award extra lives
                }
            }
        }
    }

    // Background timer task - keeps running until cancelled. It exists to
    // provide a cancellable async wait and would be a good place to update UI
    // on a different thread if needed. Currently it simply delays in a loop.
    private async Task RunTimerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
            await Task.Delay(1000);
    }

    // Add additional time to the remaining time budget
    private void AddExtraTime(TimeSpan additionalTime) =>
        _extraTime += additionalTime;

    // Compute the remaining time based on the stopwatch and any extra time
    private TimeSpan GetRemainingTime()
    {
        TimeSpan elapsed = _stopwatch.Elapsed;
        TimeSpan totalTime = TimeSpan.FromMinutes(2); // Total time allowed for survival mode
        TimeSpan remaining = totalTime - elapsed + _extraTime;

        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    // Reveal the guessed letter positions in the masked word
    private string UpdateHiddenWord(string currentGuess, string secretWord, char guessedLetter)
    {
        char[] updateGuess = currentGuess.ToCharArray();

        for (int i = 0; i < secretWord.Length; i++)
        {
            if (secretWord[i] == guessedLetter)
            {
                updateGuess[i] = guessedLetter;
            }
        }
        return new string(updateGuess);
    }

    // Create an initial masked string of underscores matching the secret word length
    private string HiddenWord(string secretWord) =>
        new string('_', secretWord.Length);

    // Choose the next secret word randomly from the available word list and remove it
    private string NextSecretWord()
    {
        if (_wordList.Count == 0) return null;

        int i = rnd.Next(0, _wordList.Count);
        string word = _wordList[i];
        _wordList.RemoveAt(i);
        return word;
    }

    // Return total game duration in seconds (rounded to two decimals)
    private double GetTotalGameDuration() =>
        Math.Round(_stopwatch.Elapsed.TotalSeconds, 2);
}
